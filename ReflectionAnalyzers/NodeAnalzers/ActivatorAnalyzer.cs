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
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                invocation.TryGetTarget(KnownSymbol.Activator.CreateInstance, context.SemanticModel, context.CancellationToken, out var createInstance))
            {
                if (createInstance.IsGenericMethod)
                {
                    if (memberAccess.Name is GenericNameSyntax genericName &&
                        genericName.TypeArgumentList is TypeArgumentListSyntax typeArgumentList &&
                        typeArgumentList.Arguments.TrySingle(out var typeArgument) &&
                        context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out var type) &&
                        type is INamedTypeSymbol namedType &&
                        !namedType.Constructors.TrySingle(x => x.Parameters.Length == 0 && x.DeclaredAccessibility == Accessibility.Public, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, typeArgument.GetLocation(), type.ToDisplayString()));
                    }
                }
                else if (createInstance.TryFindParameter(KnownSymbol.Type, out var typeParameter) &&
                        invocation.TryFindArgument(typeParameter, out var typeArgument) &&
                         Type.TryGet(typeArgument.Expression, context, out var type, out var typeSource) &&
                         type is INamedTypeSymbol namedType)
                {
                    if (createInstance.Parameters.Length == 1 &&
                        !namedType.Constructors.TrySingle(x => x.Parameters.Length == 0 && x.DeclaredAccessibility == Accessibility.Public, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, GetLocation(typeSource) ?? typeArgument.GetLocation(), type.ToDisplayString()));
                    }
                    else if (createInstance.Parameters.Length == 2 &&
                             createInstance.Parameters.TryElementAt(1, out var parameter))
                    {
                        if (parameter.Type == KnownSymbol.Boolean &&
                            invocation.TryFindArgument(parameter, out var flagArg) &&
                            flagArg.Expression is LiteralExpressionSyntax literal)
                        {
                            if (literal.IsKind(SyntaxKind.TrueLiteralExpression) &&
                                !namedType.Constructors.TrySingle(x => x.Parameters.Length == 0, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, GetLocation(typeSource) ?? typeArgument.GetLocation(), type.ToDisplayString()));
                            }
                            else if (literal.IsKind(SyntaxKind.FalseLiteralExpression) &&
                                !namedType.Constructors.TrySingle(x => x.Parameters.Length == 0 && x.DeclaredAccessibility == Accessibility.Public, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, GetLocation(typeSource) ?? typeArgument.GetLocation(), type.ToDisplayString()));
                            }
                        }
                        else if (parameter.IsParams)
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

        private static bool TryGetValues(ArgumentListSyntax argumentList, int startIndex, SyntaxNodeAnalysisContext context, out ImmutableArray<ExpressionSyntax> values)
        {
            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
            switch (argumentList.Arguments[startIndex].Expression)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                    return false;
                case ImplicitArrayCreationExpressionSyntax _:
                    break;
                case ExpressionSyntax expression when Array.TryGetValues(expression, context, out values):
                    return true;
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
            foreach (var constructor in type.Constructors)
            {
                if (constructor.IsStatic)
                {
                    continue;
                }

                switch (IsMatch(constructor.Parameters))
                {
                    case true:
                        return true;
                    case null:
                        return null;
                }
            }

            return false;
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
                    var conversion = context.SemanticModel.ClassifyConversion(values[i], parameters[i].Type);
                    if (!conversion.IsIdentity &&
                        !conversion.IsImplicit)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static Location GetLocation(Optional<ExpressionSyntax> typeSource)
        {
            if (typeSource.HasValue)
            {
                if (typeSource.Value is TypeOfExpressionSyntax typeOf &&
                    typeOf.Type is TypeSyntax typeSyntax)
                {
                    return typeSyntax.GetLocation();
                }

                return typeSource.Value.GetLocation();
            }

            return null;
        }
    }
}
