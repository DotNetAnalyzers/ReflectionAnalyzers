namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly CodeFixProvider Fix = new ArgumentFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL005");

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Instance",                                "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",        "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.NonPublic | BindingFlags.Instance",                             "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",  "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly",   "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.ToString", "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance")]
        public void GetMethod(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), ↓BindingFlags.Public | BindingFlags.Static);
        }

        public static int Static() => 0;

        public int Public() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Static", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);
            var message = $"There is no member matching the name and binding flags. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
