namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL003MemberDoesNotExist.Descriptor);

        [Test]
        public void GetPropertyAnonymousType()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty(↓""MISSING"");
        }
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

    public sealed class CustomAggregateException : AggregateException
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

        [TestCase("GetNestedType(↓\"Generic\", BindingFlags.Public)")]
        [TestCase("GetNestedType(↓nameof(Generic<int>), BindingFlags.Public)")]
        [TestCase("GetNestedType(↓\"Generic`2\", BindingFlags.Public)")]
        public void GetNestedType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(↓nameof(Generic<int>), BindingFlags.Public);
        }

        public class Generic<T>
        {
        }
    }
}".AssertReplace("GetNestedType(↓nameof(Generic<int>), BindingFlags.Public)", call);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
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

    public sealed class Foo
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

        [Test]
        public void GetTupleFieldItem1ByName()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => Create().GetType().GetField(↓""a"");


        static (int a, int b) Create() => default;
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("a1")]
        [TestCase("a7")]
        [TestCase("a8")]
        public void GetTupleFieldItem7ByName(string field)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => Create().GetType().GetField(↓""a7"");


        static (int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8) Create() => default;
    }
}".AssertReplace("a7", field);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(↓\"get_P\")")]
        public void MissingGetter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).GetProperty(nameof(P)).↓GetMethod;


        public int P { set; }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(P)).↓GetMethod", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(↓\"set_P\")")]
        public void MissingSetter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).GetProperty(nameof(P)).↓SetMethod;


        public int P { get; }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(P)).↓SetMethod", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
