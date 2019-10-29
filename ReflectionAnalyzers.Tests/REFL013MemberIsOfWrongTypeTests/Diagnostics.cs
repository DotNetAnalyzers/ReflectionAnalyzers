namespace ReflectionAnalyzers.Tests.REFL013MemberIsOfWrongTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL013MemberIsOfWrongType.Descriptor);

        [TestCase("GetEvent(nameof(this.P))")]
        [TestCase("GetEvent(nameof(this.P), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(nameof(this.P))")]
        [TestCase("GetField(nameof(this.P), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.P))")]
        [TestCase("GetMethod(nameof(this.P), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(this.P))")]
        [TestCase("GetNestedType(nameof(this.P), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenMatchIsProperty(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).↓GetMethod(nameof(this.P));
        }

        public int P { get; }
    }
}".AssertReplace("GetMethod(nameof(this.P))", call);
            var message = "The type RoslynSandbox.C has a property named P.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetEvent(nameof(this.M))")]
        [TestCase("GetEvent(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(nameof(this.M))")]
        [TestCase("GetField(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetProperty(nameof(this.M))")]
        [TestCase("GetProperty(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(this.M))")]
        [TestCase("GetNestedType(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetPropertyMatchingMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).↓GetProperty(nameof(this.M));
        }

        public int M() => 0;
    }
}".AssertReplace("GetProperty(nameof(this.M))", call);
            var message = "The type RoslynSandbox.C has a method named M.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
