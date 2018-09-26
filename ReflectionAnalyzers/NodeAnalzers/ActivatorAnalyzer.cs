namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ActivatorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL025ActivatorCreateInstanceArguments.Descriptor,
            REFL026MissingDefaultConstructor.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.TryGetTarget(KnownSymbol.Activator.CreateInstance, context.SemanticModel, context.CancellationToken, out var createInstance))
            {
                if (IsMissingDefaultConstructor(createInstance, invocation, context, out var location, out var createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, location, createdType.ToDisplayString()));
                }
                else if (createInstance.Parameters.Length > 1 &&
                    createInstance.TryFindParameter(KnownSymbol.Type, out var typeParameter) &&
                        invocation.TryFindArgument(typeParameter, out var typeArgument) &&
                         Type.TryGet(typeArgument.Expression, context, out var type, out _) &&
                         type is INamedTypeSymbol namedType)
                {
                    if (createInstance.Parameters.Length == 2 &&
                             createInstance.Parameters.TryElementAt(1, out var parameter))
                    {
                        if (parameter.IsParams)
                        {
                            if (invocation.ArgumentList.Arguments[1].Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL025ActivatorCreateInstanceArguments.Descriptor, invocation.ArgumentList.Arguments[1].Expression.GetLocation()));
                            }
                            else if (TryGetValues(argumentList, 1, context, out var values) &&
                                     TryFindConstructor(namedType, values, context) == false)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL025ActivatorCreateInstanceArguments.Descriptor, invocation.ArgumentList.Arguments[1].GetLocation()));
                            }
                        }
                    }
                }
            }
        }

        private static bool IsMissingDefaultConstructor(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out Location location, out ITypeSymbol createdType)
        {
            if (createInstance.IsGenericMethod &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName &&
                genericName.TypeArgumentList is TypeArgumentListSyntax typeArgumentList &&
                typeArgumentList.Arguments.TrySingle(out var typeArgument) &&
                context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out createdType) &&
                !HasDefaultConstructor(createdType))
            {
                location = typeArgument.GetLocation();
                return true;
            }

            if (createInstance.Parameters.TrySingle(out var typeParameter) &&
                typeParameter.Type == KnownSymbol.Type &&
                invocation.TryFindArgument(typeParameter, out var typeArg) &&
                Type.TryGet(typeArg.Expression, context, out createdType, out var typeSource) &&
                !HasDefaultConstructor(createdType))
            {
                location = GetLocation(typeSource);
                return true;
            }

            if (createInstance.Parameters.Length == 2 &&
                createInstance.Parameters.TryElementAt(0, out typeParameter) &&
                typeParameter.Type == KnownSymbol.Type &&
                invocation.TryFindArgument(typeParameter, out typeArg) &&
                Type.TryGet(typeArg.Expression, context, out createdType, out typeSource) &&
                createInstance.Parameters.TryElementAt(1, out var flagParameter) &&
                flagParameter.Type == KnownSymbol.Boolean &&
                invocation.TryFindArgument(flagParameter, out var flagArg))
            {
                switch (flagArg.Expression)
                {
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.TrueLiteralExpression) &&
                                                              Type.HasVisibleNonPublicMembers(createdType, recursive: false) &&
                                                              createdType is INamedTypeSymbol namedType &&
                                                              !namedType.Constructors.TrySingle(
                                                                  x => x.Parameters.Length == 0 &&
                                                                       !x.IsStatic,
                                                                  out _)
                                                               :
                        location = GetLocation(typeSource);
                        return true;
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression) &&
                                                              !HasDefaultConstructor(createdType):
                        location = GetLocation(typeSource);
                        return true;
                }
            }

            location = null;
            createdType = null;
            return false;

            bool HasDefaultConstructor(ITypeSymbol type)
            {
                return type is INamedTypeSymbol namedType &&
                       namedType.Constructors.TrySingle(
                           x => x.Parameters.Length == 0 &&
                                x.DeclaredAccessibility == Accessibility.Public &&
                                !x.IsStatic,
                           out _);
            }

            Location GetLocation(Optional<ExpressionSyntax> source)
            {
                if (source.HasValue)
                {
                    if (source.Value is TypeOfExpressionSyntax typeOf &&
                        typeOf.Type is TypeSyntax typeSyntax)
                    {
                        return typeSyntax.GetLocation();
                    }

                    return source.Value.GetLocation();
                }

                return null;
            }
        }

        private static bool TryGetValues(ArgumentListSyntax argumentList, int startIndex, SyntaxNodeAnalysisContext context, out ImmutableArray<ExpressionSyntax> values)
        {
            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
            switch (argumentList.Arguments[startIndex].Expression)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                    return false;
                case ExpressionSyntax expression when context.SemanticModel.TryGetType(expression, context.CancellationToken, out var type) &&
                                                      type is IArrayTypeSymbol arrayType &&
                                                      arrayType.ElementType == KnownSymbol.Object:
                    return Array.TryGetValues(expression, context, out values);
            }

            for (var i = startIndex; i < argumentList.Arguments.Count; i++)
            {
                builder.Add(argumentList.Arguments[i].Expression);
            }

            values = builder.ToImmutable();
            return true;
        }

        private static bool? TryFindConstructor(INamedTypeSymbol type, ImmutableArray<ExpressionSyntax> values, SyntaxNodeAnalysisContext context)
        {
            var foundMatch = false;
            foreach (var constructor in type.Constructors)
            {
                if (constructor.IsStatic)
                {
                    continue;
                }

                switch (IsMatch(constructor.Parameters))
                {
                    case true:
                        if (foundMatch)
                        {
                            return false;
                        }

                        foundMatch = true;
                        break;
                    case null:
                        return null;
                }
            }

            return foundMatch;
            bool? IsMatch(ImmutableArray<IParameterSymbol> parameters)
            {
                if (parameters.TryLast(out var last) &&
                    last.IsParams)
                {
                    return null;
                }

                if (parameters.TryFirst(x => x.RefKind == RefKind.Ref || x.RefKind == RefKind.Out, out _))
                {
                    return null;
                }

                if (values.Length != parameters.Length)
                {
                    return false;
                }

                for (var i = 0; i < values.Length; i++)
                {
                    if (IsNull(values[i]))
                    {
                        continue;
                    }

                    var conversion = context.SemanticModel.ClassifyConversion(values[i], parameters[i].Type);
                    if (!conversion.Exists)
                    {
                        return false;
                    }

                    if (conversion.IsImplicit ||
                        conversion.IsIdentity)
                    {
                        continue;
                    }

                    if (conversion.IsExplicit &&
                        conversion.IsReference)
                    {
                        continue;
                    }

                    return false;
                }

                return true;
            }

            bool IsNull(ExpressionSyntax expression)
            {
                switch (expression)
                {
                    case LiteralExpressionSyntax literal:
                        return literal.IsKind(SyntaxKind.NullLiteralExpression);
                    case CastExpressionSyntax cast:
                        return IsNull(cast.Expression);
                    case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                        return IsNull(binary.Left);
                    default:
                        return false;
                }
            }
        }
    }
}
