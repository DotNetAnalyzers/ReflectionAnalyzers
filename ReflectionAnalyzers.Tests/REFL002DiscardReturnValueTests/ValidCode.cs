namespace ReflectionAnalyzers.Tests.REFL002DiscardReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL002DiscardReturnValue.Descriptor);

        [TestCase("_ = ")]
        [TestCase("var _ = ")]
        [TestCase("var __ = ")]
        [TestCase("")]
        public void Discarding(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("_ = ", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("Assert.Null(typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null))")]
        [TestCase("Assert.IsNull(typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null))")]
        [TestCase("Assert.AreEqual(null, typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null))")]
        public void WhenUsedInAssert(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Foo
    {
        public Foo()
        {
            Assert.Null(typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null));
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("Assert.Null(typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null))", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void AssigningLocal()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void AssigningField()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly int value;

        public Foo()
        {
            this.value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void UsingInExpression()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var text = ((int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null)).ToString();
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
