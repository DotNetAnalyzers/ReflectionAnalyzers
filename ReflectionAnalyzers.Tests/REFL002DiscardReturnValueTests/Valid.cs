namespace ReflectionAnalyzers.Tests.REFL002DiscardReturnValueTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly InvokeAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL002DiscardReturnValue;

    [TestCase("_ = ")]
    [TestCase("var _ = ")]
    [TestCase("var __ = ")]
    [TestCase("")]
    public static void Discarding(string call)
    {
        var code = @"
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public C()
        {
            _ = typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static void M()
        {
        }
    }
}".AssertReplace("_ = ", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("Assert.Null(typeof(C).GetMethod(nameof(M)).Invoke(null, null))")]
    [TestCase("Assert.IsNull(typeof(C).GetMethod(nameof(M)).Invoke(null, null))")]
    [TestCase("Assert.AreEqual(null, typeof(C).GetMethod(nameof(M)).Invoke(null, null))")]
    public static void WhenUsedInAssert(string call)
    {
        var code = @"
#pragma warning disable CS8602
namespace N
{
    using NUnit.Framework;

    public class C
    {
        public C()
        {
            Assert.Null(typeof(C).GetMethod(nameof(M)).Invoke(null, null));
        }

        public static void M()
        {
        }
    }
}".AssertReplace("Assert.Null(typeof(C).GetMethod(nameof(M)).Invoke(null, null))", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void AssigningLocal()
    {
        var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void AssigningField()
    {
        var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        private readonly int value;

        public C()
        {
            this.value = (int)typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void UsingInExpression()
    {
        var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public C()
        {
            var text = ((int)typeof(C).GetMethod(nameof(M)).Invoke(null, null)).ToString();
        }

        public static int M() => 0;
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
}
