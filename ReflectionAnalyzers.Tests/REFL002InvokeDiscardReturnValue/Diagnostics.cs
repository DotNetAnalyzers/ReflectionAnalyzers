namespace ReflectionAnalyzers.Tests.REFL002InvokeDiscardReturnValue
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL002");
    }
}
