namespace ReflectionAnalyzers.Tests.REFL002DiscardReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL002DiscardReturnValue.Descriptor;

        [TestCase("_ = ")]
        [TestCase("var _ = ")]
        [TestCase("var __ = ")]
        [TestCase("")]
        public void Discarding(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            _ = typeof(C).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("_ = ", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Assert.Null(typeof(C).GetMethod(nameof(Bar)).Invoke(null, null))")]
        [TestCase("Assert.IsNull(typeof(C).GetMethod(nameof(Bar)).Invoke(null, null))")]
        [TestCase("Assert.AreEqual(null, typeof(C).GetMethod(nameof(Bar)).Invoke(null, null))")]
        public void WhenUsedInAssert(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class C
    {
        public C()
        {
            Assert.Null(typeof(C).GetMethod(nameof(Bar)).Invoke(null, null));
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("Assert.Null(typeof(C).GetMethod(nameof(Bar)).Invoke(null, null))", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void AssigningLocal()
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void AssigningField()
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        private readonly int value;

        public C()
        {
            this.value = (int)typeof(C).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void UsingInExpression()
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var text = ((int)typeof(C).GetMethod(nameof(Bar)).Invoke(null, null)).ToString();
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
