namespace ReflectionAnalyzers.Tests.REFL009MemberCantBeFoundTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL009MemberCantBeFound.Descriptor);

        [TestCase("typeof(Foo).GetMethod(↓\"MISSING\")")]
        [TestCase("new Foo().GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("this.GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("GetType().GetMethod(↓\"MISSING\")")]
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
            var message = "The referenced member MISSING is not known to exist in RoslynSandbox.Foo.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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

        [Test]
        public void SubclassAggregateExceptionGetFieldDeclaredOnly()
        {
            var exception = @"
namespace RoslynSandbox
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }
}";
            var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(CustomAggregateException).GetField(↓""MISSING"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, exception, code);
        }

        [TestCase("GetProperty(↓\"Item\")")]
        [TestCase("GetProperty(↓\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void NamedIndexer(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetProperty(""Item"");
        }

        [IndexerName(""Bar"")]
        public int this[int i] => 0;
    }
}".AssertReplace("GetProperty(\"Item\")", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
