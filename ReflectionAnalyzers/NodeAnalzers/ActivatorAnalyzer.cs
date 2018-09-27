namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
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
            REFL025ArgumentsDontMatchParameters.Descriptor,
            REFL026MissingDefaultConstructor.Descriptor,
            REFL028CastReturnValueToCorrectType.Descriptor);

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
                invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Activator.CreateInstance, context.SemanticModel, context.CancellationToken, out var createInstance) &&
                TryGetCreatedType(createInstance, invocation, context, out var createdType, out var typeSource))
            {
                if (IsMissingDefaultConstructor(createInstance, invocation, createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, typeSource.GetLocation(), createdType.ToDisplayString()));
                }
                else if (IsArgumentMisMatch(createInstance, invocation, createdType, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, invocation.ArgumentList.Arguments[1].Expression.GetLocation()));
                }

                if (!createInstance.IsGenericMethod &&
                    createdType != null)
                {
                    switch (invocation.Parent)
                    {
                        case CastExpressionSyntax castExpression when castExpression.Type is TypeSyntax typeSyntax &&
                                                                      context.SemanticModel.TryGetType(typeSyntax, context.CancellationToken, out var castType) &&
                                                                      !createdType.IsAssignableTo(castType, context.Compilation):
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL028CastReturnValueToCorrectType.Descriptor,
                                    typeSyntax.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(TypeSyntax),
                                        createdType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)),
                                    createdType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart)));
                            break;
                    }
                }
            }
        }

        private static bool TryGetCreatedType(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out INamedTypeSymbol createdType, out ExpressionSyntax typeSource)
        {
            if (createInstance.IsGenericMethod &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName &&
                genericName.TypeArgumentList is TypeArgumentListSyntax typeArgumentList &&
                typeArgumentList.Arguments.TrySingle(out var typeArgument) &&
                context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out var type))
            {
                typeSource = typeArgument;
                createdType = type as INamedTypeSymbol;
                return type != null;
            }

            if (createInstance.TryFindParameter(KnownSymbol.Type, out var typeParameter) &&
               invocation.TryFindArgument(typeParameter, out var typeArg) &&
               Type.TryGet(typeArg.Expression, context, out type, out var optionalTypeSource))
            {
                if (optionalTypeSource.HasValue)
                {
                    switch (optionalTypeSource.Value)
                    {
                        case TypeOfExpressionSyntax typeOf:
                            typeSource = typeOf.Type;
                            break;
                        default:
                            typeSource = optionalTypeSource.Value;
                            break;
                    }
                }
                else
                {
                    typeSource = typeArg.Expression;
                }

                createdType = type as INamedTypeSymbol;
                return type != null;
            }

            createdType = null;
            typeSource = null;
            return false;
        }

        private static bool IsMissingDefaultConstructor(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, INamedTypeSymbol createdType)
        {
            if (createInstance.IsGenericMethod &&
                !HasDefaultConstructor())
            {
                return true;
            }

            if (createInstance.Parameters.TrySingle(out _) &&
                !HasDefaultConstructor())
            {
                return true;
            }

            if (createInstance.Parameters.Length == 2 &&
                createInstance.Parameters.TryElementAt(0, out var typeParameter) &&
                typeParameter.Type == KnownSymbol.Type &&
                createInstance.Parameters.TryElementAt(1, out var flagParameter) &&
                flagParameter.Type == KnownSymbol.Boolean &&
                invocation.TryFindArgument(flagParameter, out var flagArg))
            {
                switch (flagArg.Expression)
                {
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.TrueLiteralExpression) &&
                                                              Type.HasVisibleNonPublicMembers(createdType, recursive: false) &&
                                                              !createdType.Constructors.TrySingle(
                                                                  x => x.Parameters.Length == 0 &&
                                                                       !x.IsStatic,
                                                                  out _)
                                                               :
                        return true;
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression) &&
                                                              !HasDefaultConstructor():
                        return true;
                }
            }

            return false;

            bool HasDefaultConstructor()
            {
                return createdType.Constructors.TrySingle(
                           x => x.Parameters.Length == 0 &&
                                x.DeclaredAccessibility == Accessibility.Public &&
                                !x.IsStatic,
                           out _);
            }
        }

        private static bool IsArgumentMisMatch(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, INamedTypeSymbol createdType, SyntaxNodeAnalysisContext context)
        {
            if (invocation.ArgumentList is ArgumentListSyntax argumentList &&
                createInstance.Parameters.Length > 1 &&
                createInstance.Parameters.Length == 2 &&
                createInstance.Parameters.TryElementAt(1, out var parameter) && parameter.IsParams)
            {
                if (invocation.ArgumentList.Arguments[1].Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                {
                    return !createdType.Constructors.TrySingle(x => x.Parameters.TrySingle(out parameter) && parameter.IsParams, out _);
                }

                if (TryGetValues(argumentList, 1, context, out var values) &&
                    TryFindConstructor(createdType, values, context) == false)
                {
                    return true;
                }
            }

            return false;
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
                if (parameters.TryFirst(x => x.RefKind == RefKind.Ref || x.RefKind == RefKind.Out, out _))
                {
                    return null;
                }

                if (values.Length != parameters.Length)
                {
                    if (parameters.TryLast(out var last) &&
                        !last.IsParams)
                    {
                        return false;
                    }

                    if (values.Length < parameters.Length - 1)
                    {
                        return false;
                    }
                }

                IParameterSymbol lastParameter = null;
                for (var i = 0; i < values.Length; i++)
                {
                    if (lastParameter == null ||
                        !lastParameter.IsParams)
                    {
                        if (!parameters.TryElementAt(i, out lastParameter))
                        {
                            return false;
                        }
                    }

                    if (IsNull(values[i]))
                    {
                        if (lastParameter.IsParams)
                        {
                            return false;
                        }

                        continue;
                    }

                    var conversion = context.SemanticModel.ClassifyConversion(values[i], lastParameter.Type);
                    if (!conversion.Exists)
                    {
                        if (lastParameter.Type is IArrayTypeSymbol arrayType &&
                            context.SemanticModel.ClassifyConversion(values[i], arrayType.ElementType).IsIdentity)
                        {
                            continue;
                        }

                        return false;
                    }

                    if (conversion.IsIdentity || conversion.IsImplicit)
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
