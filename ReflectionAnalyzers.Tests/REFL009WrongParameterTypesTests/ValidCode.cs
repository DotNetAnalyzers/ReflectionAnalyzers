namespace ReflectionAnalyzers.Tests.REFL009WrongParameterTypesTests
{
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
    }
}
