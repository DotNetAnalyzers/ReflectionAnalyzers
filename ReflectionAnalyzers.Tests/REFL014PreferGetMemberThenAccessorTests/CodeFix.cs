namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseGetMemberThenAccessorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL014PreferGetMemberThenAccessor.DiagnosticId);

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
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
            var methodInfo = typeof(Foo).↓GetMethod(""get_PublicGetSet"");
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }

        private int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }

        private int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);
            var message = $"Prefer typeof(Foo).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public void StaticPropertyInSameType(string before, string after)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).↓GetMethod(""get_PublicGetSet"");
        }

        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }

        private static int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod;
        }

        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }

        private static int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod", after);
            var message = $"Prefer typeof(Foo).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                     "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(Foo.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(Foo.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                     "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(Foo.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(Foo.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void InstancePropertyInOtherType(string before, string after)
        {
            var foo = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }
        
        private int PrivateGetSet { get; set; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(Foo).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);

            var message = $"Prefer typeof(Foo).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { foo, code }, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(Foo.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(Foo.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(Foo.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(Foo.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public void StaticPropertyInOtherType(string before, string after)
        {
            var foo = @"
namespace RoslynSandbox
{
    public static class Foo
    {
        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }
        
        private static int PrivateGetSet { get; set; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(Foo).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(Foo.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);

            var message = $"Prefer typeof(Foo).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { foo, code }, fixedCode);
        }
    }
}
