namespace ReflectionAnalyzers.Tests.REFL039PreferTypeofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseTypeOfFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL039PreferTypeof);

        [TestCase("string", "string")]
        [TestCase("int", "int")]
        [TestCase("int?", "int")]
        [TestCase("(int, int)", "(int, int)")]
        [TestCase("(int, int)?", "(int, int)")]
        [TestCase("StringComparison", "StringComparison")]
        [TestCase("StringComparison?", "StringComparison")]
        public static void WhenCallingGetType(string parameterType, string type)
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public static object M(int? value) => value.GetType();
    }
}".AssertReplace("int?", parameterType);

            var after = @"
namespace N
{
    using System;

    public class C
    {
        public static object M(int? value) => typeof(int);
    }
}".AssertReplace("int?", parameterType)
  .AssertReplace("typeof(int)", $"typeof({type})");

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
