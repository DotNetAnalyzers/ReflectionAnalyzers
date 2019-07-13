namespace ReflectionAnalyzers.Tests.REFL007BindingFlagsOrderTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new BindingFlagsAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL007");

        [TestCase("BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("BindingFlags.Instance | BindingFlags.Public",                                "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public static void GetMethod(string flags, string expected)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), ↓BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

        public int Bar() => 0;
    }
}".AssertReplace("BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly", flags);
            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int Bar() => 0;
    }
}".AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"The binding flags are not in the expected order. Expected: {expected}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }
    }
}
