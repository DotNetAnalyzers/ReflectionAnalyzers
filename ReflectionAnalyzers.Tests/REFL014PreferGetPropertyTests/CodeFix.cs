namespace ReflectionAnalyzers.Tests.REFL014PreferGetPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseGetPropertyAccessorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL014PreferGetProperty.DiagnosticId);

        [Test]
        public void PublicGetSetInstanceGetter()
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

        public int Value { get; set; }
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
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }

        public int Value { get; set; }
    }
}";
            var message = "Prefer typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod.";
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
        public void PublicGetSetInstanceSetter()
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

        public int Value { get; set; }
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
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod;
        }

        public int Value { get; set; }
    }
}";
            var message = "Prefer typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod.";
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

        [Test]
        public void PublicGetInternalSetInstanceSetter()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = ↓typeof(Foo).GetMethod(""set_Value"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int Value { get; internal set; }
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
            var methodInfo = typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod;
        }

        public int Value { get; internal set; }
    }
}";
            var message = "Prefer typeof(Foo).GetProperty(nameof(Foo.Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
