namespace ReflectionAnalyzers.Tests.REFL011DuplicateBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new BindingFlagsAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL011");

        [TestCase("BindingFlags.Public | BindingFlags.↓Public",                                                   "BindingFlags.Public")]
        [TestCase("System.Reflection.BindingFlags.Public | BindingFlags.↓Public",                                 "System.Reflection.BindingFlags.Public")]
        [TestCase("System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.↓Public",               "System.Reflection.BindingFlags.Public")]
        [TestCase("BindingFlags.Public | BindingFlags.Static | BindingFlags.↓Public | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("BindingFlags.Public | BindingFlags.↓Public | BindingFlags.Static | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        public static void GetMethod(string flags, string expected)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Instance | BindingFlags.Public | BindingFlags.↓Public  | BindingFlags.DeclaredOnly);
        }

        public int M() => 0;
    }
}".AssertReplace("BindingFlags.Instance | BindingFlags.Public | BindingFlags.↓Public  | BindingFlags.DeclaredOnly", flags);
            var after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int M() => 0;
    }
}".AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = "Duplicate flag.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("Public | ↓Public",                                "Public")]
        [TestCase("System.Reflection.BindingFlags.Public | ↓Public", "System.Reflection.BindingFlags.Public")]
        public static void GetMethodUsingStatic(string flags, string expected)
        {
            var before = @"
namespace N
{
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Instance | BindingFlags.Public | BindingFlags.↓Public  | BindingFlags.DeclaredOnly);
        }

        public int M() => 0;
    }
}".AssertReplace("BindingFlags.Instance | BindingFlags.Public | BindingFlags.↓Public  | BindingFlags.DeclaredOnly", flags);
            var after = @"
namespace N
{
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int M() => 0;
    }
}".AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = "Duplicate flag.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }
    }
}
