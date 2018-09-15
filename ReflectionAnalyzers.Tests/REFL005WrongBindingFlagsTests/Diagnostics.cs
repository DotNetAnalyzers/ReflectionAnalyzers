namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using System.Reflection;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
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
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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
            var message = "There is no member matching the name and binding flags. Expected: BindingFlags.Public | BindingFlags.Instance.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
