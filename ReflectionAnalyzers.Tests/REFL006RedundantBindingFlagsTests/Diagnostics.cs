namespace ReflectionAnalyzers.Tests.REFL006RedundantBindingFlagsTests
{
    using System.Reflection;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL006");

        [TestCase("BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static")]
        [TestCase("BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic")]
        [TestCase("BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy")]
        [TestCase("BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase")]
        public void GetToString(string flags)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), ↓BindingFlags.Public | BindingFlags.Instance);
        }
    }
}".AssertReplace("BindingFlags.Public | BindingFlags.Instance", flags);

            var message = "The binding flags can be more precise. Expected: BindingFlags.Public | BindingFlags.Instance.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase(" BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance")]
        [TestCase(" BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic")]
        [TestCase(" BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly")]
        [TestCase(" BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase")]
        public void GetReferenceEquals(string flags)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy", flags);

            var message = "The binding flags can be more precise. Expected: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static")]
        [TestCase("BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public")]
        [TestCase("BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy")]
        [TestCase("BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase ")]
        public void GetPrivate(string flags)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        private void Bar()
        {
        }
    }
}".AssertReplace("BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", flags);

            var message = "The binding flags can be more precise. Expected: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
