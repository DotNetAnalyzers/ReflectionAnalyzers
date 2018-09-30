namespace ReflectionAnalyzers.Tests.REFL009WrongTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL009MemberCantBeFound.Descriptor);
    }
}
