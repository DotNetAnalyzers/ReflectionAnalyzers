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
                    context.SemanticModel.TryGetType(paramsArg.Expression, context.CancellationToken, out var type) &&
                    type is IArrayTypeSymbol arrayType &&
                    arrayType.ElementType == KnownSymbol.Object &&
                    Array.IsCreatingEmpty(paramsArg.Expression, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL024PreferNullOverEmptyArray.Descriptor, paramsArg.GetLocation()));
                }
            }
        }
    }
}
