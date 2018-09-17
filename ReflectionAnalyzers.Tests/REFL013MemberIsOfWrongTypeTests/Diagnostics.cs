namespace ReflectionAnalyzers.Tests.REFL013MemberIsOfWrongTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL013MemberIsOfWrongType.DiagnosticId);

        [Test]
        public void GetMethodMatchingProperty()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).↓GetMethod(nameof(this.Bar));
        }

        public int Bar { get; }
    }
}";
            var message = "The type RoslynSandbox.Foo has a member named Bar of type SourcePropertySymbol.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void GetPropertyMatchingMethod()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).↓GetProperty(nameof(this.Bar));
        }

        public int Bar() => 0;
    }
}";
            var message = "The type RoslynSandbox.Foo has a member named Bar of type SourceMemberMethodSymbol.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
