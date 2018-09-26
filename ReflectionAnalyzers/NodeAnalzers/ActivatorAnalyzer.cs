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
                        else if(parameter.IsParams)
                        {
                            
                        }
                    }
                }
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
