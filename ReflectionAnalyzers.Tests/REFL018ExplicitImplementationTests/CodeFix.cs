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

        [TestCase("GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitImplementation(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(IConvertible).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);

            var message = "ToBoolean is explicitly implemented.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
