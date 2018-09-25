namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvokeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL002InvokeDiscardReturnValue.Descriptor,
            REFL024PreferNullOverEmptyArray.Descriptor);

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
                invocation.TryGetMethodName(out var name) &&
                name == "Invoke" &&
                context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var invoke) &&
                invoke.ContainingType.IsAssignableTo(KnownSymbol.MemberInfo, context.Compilation))
            {
                if (invoke.TryFindParameter("parameters", out var parameter) &&
                    invocation.TryFindArgument(parameter, out var paramsArg) &&
                    IsEmptyArray(paramsArg, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL024PreferNullOverEmptyArray.Descriptor, paramsArg.GetLocation()));
                }
            }
        }

        private static bool IsEmptyArray(ArgumentSyntax argument, SyntaxNodeAnalysisContext context)
        {
            switch (argument.Expression)
            {
                case InvocationExpressionSyntax invocation when context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var symbol) &&
                                                                symbol == KnownSymbol.Array.Empty:
                    return true;
                case ArrayCreationExpressionSyntax arrayCreation:
                    if (arrayCreation.Type is ArrayTypeSyntax arrayType)
                    {
                        foreach (var rankSpecifier in arrayType.RankSpecifiers)
                        {
                            foreach (var size in rankSpecifier.Sizes)
                            {
                                if (size is LiteralExpressionSyntax literal &&
                                    literal.Token.ValueText != "0")
                                {
                                    return false;
                                }
                            }
                        }

                        var initializer = arrayCreation.Initializer;
                        return initializer == null ||
                               initializer.Expressions.Count == 0;
                    }

                    return false;
                default:
                    return false;
            }
        }
    }
}
