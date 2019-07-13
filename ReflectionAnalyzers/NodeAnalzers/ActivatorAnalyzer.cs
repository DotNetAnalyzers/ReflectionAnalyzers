namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ActivatorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL001CastReturnValue.Descriptor,
            REFL025ArgumentsDontMatchParameters.Descriptor,
            REFL026NoDefaultConstructor.Descriptor,
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
                if (!createInstance.IsGenericMethod &&
                    ReturnValue.ShouldCast(invocation, createdType, context))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL001CastReturnValue.Descriptor,
                            invocation.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(TypeSyntax),
                                createdType.ToString(context)),
                            createdType.ToString(context)));
                }

                if (createdType is INamedTypeSymbol namedType)
                {
                    if (IsMissingDefaultConstructor(createInstance, invocation, namedType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL026NoDefaultConstructor.Descriptor, typeSource.GetLocation(), createdType.ToDisplayString()));
                    }
                    else if (IsArgumentMisMatch(createInstance, invocation, namedType, context))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, invocation.ArgumentList.Arguments[1].GetLocation()));
                    }
                }

                if (!createInstance.IsGenericMethod &&
                    Type.IsCastToWrongType(invocation, createdType, context, out var typeSyntax))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL028CastReturnValueToCorrectType.Descriptor,
                            typeSyntax.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(TypeSyntax),
                                createdType.ToString(context)),
                            createdType.ToString(context)));
                }
            }
        }

        private static bool TryGetCreatedType(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol createdType, out ExpressionSyntax typeSource)
        {
            if (createInstance.IsGenericMethod &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName &&
                genericName.TypeArgumentList is TypeArgumentListSyntax typeArgumentList &&
                typeArgumentList.Arguments.TrySingle(out var typeArgument) &&
                context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out createdType))
            {
                typeSource = typeArgument;
                return true;
            }

            if (createInstance.TryFindParameter(KnownSymbol.Type, out var typeParameter) &&
                invocation.TryFindArgument(typeParameter, out var typeArg) &&
                Type.TryGet(typeArg.Expression, context, out createdType, out var source))
            {
                switch (source)
                {
                    case TypeOfExpressionSyntax typeOf:
                        typeSource = typeOf.Type;
                        break;
                    default:
                        typeSource = typeArg.Expression;
                        break;
                }

                return true;
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
                                                                  out _):
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
            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>(argumentList.Arguments.Count - startIndex);
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
            if (type == null)
            {
                return null;
            }

            var foundMatch = false;
            foreach (var constructor in type.Constructors)
            {
                if (constructor.IsStatic)
                {
                    continue;
                }

                switch (Arguments.TryFindFirstMisMatch(constructor.Parameters, values, context, out _))
                {
                    case false:
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
        }
    }
}
