namespace ReflectionAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class InvocationExpressionSyntaxExt
    {
        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to <paramref name="expected"/>.
        /// </summary>
        /// <param name="invocation">The <see cref="InvocationExpressionSyntax"/>.</param>
        /// <param name="expected">The <see cref="QualifiedMethod"/> to match against.</param>
        /// <param name="context">The <see cref="SyntaxNodeAnalysisContext"/>.</param>
        /// <param name="target">The symbol of the target method.</param>
        /// <returns>Tru if <paramref name="invocation"/> is a call to <paramref name="expected"/>.</returns>
        [Obsolete("Use from Gu.Roslyn.Extensions after update.")]
        internal static bool TryGetTarget(this InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out IMethodSymbol target)
        {
            target = null;
            return invocation.TryGetMethodName(out var name) &&
                   name == expected.Name &&
                   context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out target) &&
                   target == expected;
        }
    }
}
