namespace ReflectionAnalyzers.Tests.REFL002InvokeDiscardReturnValue
{
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
    }
}
