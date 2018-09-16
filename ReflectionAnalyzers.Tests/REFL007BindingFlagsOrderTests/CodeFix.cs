namespace ReflectionAnalyzers.Tests.REFL007BindingFlagsOrderTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL007");

        [TestCase("this.Static",      "BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.GetHashCode", "BindingFlags.Instance | BindingFlags.Public",    "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.Private",     "BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
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

        public override string ToString() => string.Empty;

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

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"The binding flags are not in the expected order. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
