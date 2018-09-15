namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly CodeFixProvider Fix = new ArgumentFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL005");

        [Test]
        public void GetPrivateBindingFlagsPublic()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), ↓BindingFlags.Instance | BindingFlags.Public);
        }

        private void Bar()
        {
        }
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        private void Bar()
        {
        }
    }
}";
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetStaticBindingFlagsInstance()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), ↓BindingFlags.Instance | BindingFlags.Public);
        }

        public static void Bar()
        {
        }
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        public static void Bar()
        {
        }
    }
}";
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetInstanceBindingFlagsStatic()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), ↓BindingFlags.Static | BindingFlags.Public);
        }

        public void Bar()
        {
        }
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public void Bar()
        {
        }
    }
}";
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetToStringWithDeclaredOnly()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), ↓BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public void Bar()
        {
        }
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance);
        }

        public void Bar()
        {
        }
    }
}";
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Instance.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
