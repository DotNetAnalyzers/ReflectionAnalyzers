namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL003");

        [TestCase("typeof(Foo).GetMethod(\"MISSING\")")]
        [TestCase("new Foo().GetType().GetMethod(\"MISSING\")")]
        [TestCase("this.GetType().GetMethod(\"MISSING\")")]
        [TestCase("GetType().GetMethod(\"MISSING\")")]
        public void MissingMethod(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(↓nameof(Foo));
        }
    }
}".AssertReplace("typeof(Foo).GetMethod(↓nameof(Foo))", type);
            var message = "The type RoslynSandbox.Foo does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("typeof(string).GetMethod(↓nameof(System.IConvertible.ToInt16))")]
        public void ExplicitInterface(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(↓nameof(System.IConvertible.ToInt16));
        }
    }
}".AssertReplace("typeof(string).GetMethod(↓nameof(System.IConvertible.ToInt16))", type);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void MissingPropertySetAccessor()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(↓""set_Bar"");
        }

        public int Bar { get; }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void MissingPropertyGetAccessor()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(↓""get_Bar"");
        }

        public int Bar { set; }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
