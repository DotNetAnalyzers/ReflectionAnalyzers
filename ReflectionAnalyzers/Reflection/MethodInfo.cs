namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct MethodInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IMethodSymbol Method;

        public MethodInfo(INamedTypeSymbol reflectedType, IMethodSymbol method)
        {
            this.ReflectedType = reflectedType;
            this.Method = method;
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out MethodInfo methodInfo)
        {
            if (expression is InvocationExpressionSyntax invocation &&
                GetX.TryMatchGetMethod(invocation, context, out var member, out _, out _, out _) &&
                member.Symbol is IMethodSymbol method)
            {
                methodInfo = new MethodInfo(member.ReflectedType, method);
                return true;
            }

            methodInfo = default(MethodInfo);
            return false;
        }
    }
}
