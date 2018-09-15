namespace ReflectionAnalyzers.Tests.REFL009WrongParameterTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL007");
    }
}
