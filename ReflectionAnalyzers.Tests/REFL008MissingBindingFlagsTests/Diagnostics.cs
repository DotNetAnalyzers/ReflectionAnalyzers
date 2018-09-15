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
        public void GetToString()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString)↓);
        }
    }
}";
            var message = "Specify binding flags for better performance and less fragile code. Expected: BindingFlags.Public | BindingFlags.Instance.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void GetReferenceEquals()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(ReferenceEquals)↓);
        }
    }
}";
            var message = "Specify binding flags for better performance and less fragile code. Expected: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void GetPrivate()
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

        private void Bar()
        {
        }
    }
}";
            var message = "Specify binding flags for better performance and less fragile code. Expected: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

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
            var message = "Specify binding flags for better performance and less fragile code. Expected: BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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
            var message = "Specify binding flags for better performance and less fragile code. Expected: BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
