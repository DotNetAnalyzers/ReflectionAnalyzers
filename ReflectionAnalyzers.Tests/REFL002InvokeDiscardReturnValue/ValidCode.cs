using Microsoft.CodeAnalysis.Diagnostics;

namespace ReflectionAnalyzers.Tests.REFL002InvokeDiscardReturnValue
{
    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
    }
}
