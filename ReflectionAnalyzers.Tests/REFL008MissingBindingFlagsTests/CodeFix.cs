namespace ReflectionAnalyzers.Tests.REFL008MissingBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL008");

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodNoFlags(string method, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public)↓);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})");

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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic | BindingFlags.Static",   "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",  "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), ↓BindingFlags.Public);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})").AssertReplace("BindingFlags.Public", flags);

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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
