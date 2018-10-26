namespace ReflectionAnalyzers.Tests.REFL039PreferTypeofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseTypeOfFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL039PreferTypeof.Descriptor);

        [TestCase("string", "string")]
        [TestCase("int", "int")]
        [TestCase("int?", "int")]
        [TestCase("(int, int)", "(int, int)")]
        [TestCase("(int, int)?", "(int, int)")]
        [TestCase("StringComparison", "StringComparison")]
        [TestCase("StringComparison?", "StringComparison")]
        public void WhenCallingGetType(string parameterType, string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object M(int? value) => value.GetType();
    }
}".AssertReplace("int?", parameterType);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object M(int? value) => typeof(int);
    }
}".AssertReplace("int?", parameterType)
  .AssertReplace("typeof(int)", $"typeof({type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
