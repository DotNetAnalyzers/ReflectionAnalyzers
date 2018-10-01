namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class Diagnostics
    {
        [TestCase("typeof(Foo).GetMethod(↓\"MISSING\")")]
        public void MissingMethodWhenKnownExactType(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public object Bar(Foo foo) => typeof(Foo).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(Foo).GetMethod(↓\"MISSING\")", type);

            var message = "The type RoslynSandbox.Foo does not have a member named MISSING.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("typeof(Foo).GetMethod(↓\"MISSING\")")]
        [TestCase("foo.GetType().GetMethod(↓\"MISSING\")")]
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
        public object Bar(Foo foo) => typeof(Foo).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(Foo).GetMethod(↓\"MISSING\"", type);

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
    }
}
