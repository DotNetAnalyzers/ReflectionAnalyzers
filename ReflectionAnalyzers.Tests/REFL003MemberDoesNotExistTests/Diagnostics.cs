namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL003MemberDoesNotExist.Descriptor);

        [TestCase("typeof(Foo).GetMethod(↓\"MISSING\")")]
        [TestCase("new Foo().GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("this.GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("GetType().GetMethod(↓\"MISSING\")")]
        public void MissingMethodWhenSealed(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    public sealed class Foo
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

        [Test]
        public void MissingMethodWhenStruct()
        {
            var code = @"
namespace RoslynSandbox
{
    public struct Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(↓""MISSING"");
        }
    }
}";
            var message = "The type RoslynSandbox.Foo does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void MissingMethodWhenStatic()
        {
            var code = @"
namespace RoslynSandbox
{
    public static class Foo
    {
        public void Bar()
        {
            var methodInfo = typeof(Foo).GetMethod(↓""MISSING"");
        }
    }
}";
            var message = "The type RoslynSandbox.Foo does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void MissingMethodWhenInterface()
        {
            var interfaceCode = @"
namespace RoslynSandbox
{
    public interface IFoo
    {
    }
}";

            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(IFoo).GetMethod(↓""MISSING"");
        }
    }
}";
            var message = "The type RoslynSandbox.IFoo does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), interfaceCode, code);
        }

        [TestCase("typeof(string).GetMethod(↓\"MISSING\")")]
        [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public void MissingMethodNotInSource(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(↓""MISSING"");
        }
    }
}".AssertReplace("typeof(string).GetMethod(↓\"MISSING\")", type);
            var message = "The type string does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void MissingPropertySetAccessor()
        {
            var code = @"
namespace RoslynSandbox
{
    public sealed class Foo
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
    public sealed class Foo
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
    }
}
