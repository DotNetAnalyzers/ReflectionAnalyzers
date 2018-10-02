namespace ReflectionAnalyzers.Tests.REFL018ExplicitImplementationTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseContainingTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL018ExplicitImplementation.Descriptor);

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitImplementation(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    sealed class Foo : IDisposable
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(IDisposable.Dispose));
        }

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    sealed class Foo : IDisposable
    {
        public Foo()
        {
            var methodInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        }

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            var message = "Dispose is explicitly implemented.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"ToBoolean\")")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("GetMethod(\"ToBoolean\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"ToBoolean\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitImplementationPublicNotInSource(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(""ToBoolean"");
        }
    }
}".AssertReplace("GetMethod(\"ToBoolean\")", call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(IConvertible).GetMethod(""ToBoolean"");
        }
    }
}".AssertReplace("GetMethod(\"ToBoolean\")", call);

            var message = "ToBoolean is explicitly implemented.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
