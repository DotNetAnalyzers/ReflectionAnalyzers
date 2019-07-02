namespace ReflectionAnalyzers.Tests.REFL013MemberIsOfWrongTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL013MemberIsOfWrongType.Descriptor);

        [TestCase("GetEvent(nameof(this.Bar))")]
        [TestCase("GetEvent(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(nameof(this.Bar))")]
        [TestCase("GetField(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.Bar))")]
        [TestCase("GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(this.Bar))")]
        [TestCase("GetNestedType(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
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
            var methodInfo = typeof(C).↓GetMethod(nameof(this.Bar));
        }

        public int Bar { get; }
    }
}".AssertReplace("GetMethod(nameof(this.Bar))", call);
            var message = "The type RoslynSandbox.C has a property named Bar.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetEvent(nameof(this.Bar))")]
        [TestCase("GetEvent(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(nameof(this.Bar))")]
        [TestCase("GetField(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetProperty(nameof(this.Bar))")]
        [TestCase("GetProperty(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(this.Bar))")]
        [TestCase("GetNestedType(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
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
            var methodInfo = typeof(C).↓GetProperty(nameof(this.Bar));
        }

        public int Bar() => 0;
    }
}".AssertReplace("GetProperty(nameof(this.Bar))", call);
            var message = "The type RoslynSandbox.C has a method named Bar.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
