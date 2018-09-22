namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseGetMemberThenAccessorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL014PreferGetMemberThenAccessor.DiagnosticId);

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void InstancePropertyInSameType(string before, string after)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).↓GetMethod(""get_Value"");
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_Value\")", before);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(this.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }
    }
}".AssertReplace("GetProperty(nameof(this.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);
            var message = $"Prefer typeof(Foo).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void PublicGetSetStaticGetter()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = ↓typeof(Foo).GetMethod(""get_Value"");
        }

        public static int Value { get; set; }
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod;
        }

        public static int Value { get; set; }
    }
}";
            var message = "Prefer typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void PublicGetSetStaticSetter()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = ↓typeof(Foo).GetMethod(""set_Value"");
        }

        public static int Value { get; set; }
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod;
        }

        public static int Value { get; set; }
    }
}";
            var message = "Prefer typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
