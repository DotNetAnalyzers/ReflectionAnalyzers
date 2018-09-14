namespace ReflectionAnalyzers.Tests.InvokeDiscardReturnValueTests
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
    }
}
