namespace ReflectionAnalyzers.Tests.REFL003GetMethodTargetDoesNotExistTests
{
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
    }
}
