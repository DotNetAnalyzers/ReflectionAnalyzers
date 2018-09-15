namespace ReflectionAnalyzers.Tests.REFL008MissingBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL008");

        [Test]
        public void GetPublicInstance()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar)↓);
        }

        public void Bar()
        {
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Specify binding flags for better performance and clearer less fragile code. Expected BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly."), code);
        }

        [Test]
        public void GetPublicStatic()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar)↓);
        }

        public static void Bar()
        {
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Specify binding flags for better performance and clearer less fragile code. Expected BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly."), code);
        }
    }
}
