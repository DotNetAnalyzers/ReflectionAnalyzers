namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL004");

        [TestCase("GetMethod↓(nameof(PublicStaticOverloaded))")]
        [TestCase("GetMethod↓(nameof(PublicStaticOverloaded), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(PublicStaticInstanceOverloaded), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.PublicInstanceOverloaded))")]
        [TestCase("GetMethod↓(nameof(this.PublicInstanceOverloaded), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod↓(nameof(this.PublicInstanceOverloaded), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.PublicPrivateInstanceOverloaded), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void GetMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod↓(nameof(this.ToString));
        }

        public static int PublicStaticOverloaded(int value) => value;

        public static double PublicStaticOverloaded(double value) => value;

        public static int PublicStaticInstanceOverloaded(int value) => value;

        public double PublicStaticInstanceOverloaded(double value) => value;

        public int PublicInstanceOverloaded(int value) => value;

        public double PublicInstanceOverloaded(double value) => value;

        public int PublicPrivateInstanceOverloaded(int value) => value;

        private double PublicPrivateInstanceOverloaded(double value) => value;
    }
}".AssertReplace("GetMethod↓(nameof(this.ToString))", call);
            var message = "More than one member is matching the criteria.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
