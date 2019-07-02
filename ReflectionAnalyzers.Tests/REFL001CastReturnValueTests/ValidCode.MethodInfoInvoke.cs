namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL001CastReturnValue.Descriptor;

            [TestCase("_ = ")]
            [TestCase("var _ = ")]
            [TestCase("var __ = ")]
            [TestCase("")]
            public static void Discarding(string call)
            {
                var code = @"
namespace RoslynSandbox
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

            [Test]
            public static void AssigningLocal()
            {
                var code = @"
namespace RoslynSandbox
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
            public static void IsPattern()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            if (typeof(C).GetMethod(nameof(M)).Invoke(null, null) is string text)
            {
            }
        }

        public static string M() => null;
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void SwitchPattern()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            switch (typeof(C).GetMethod(nameof(M)).Invoke(null, null))
            {
                case string text:
                    break;
            }
        }

        public static string M() => null;
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void AssigningField()
            {
                var code = @"
namespace RoslynSandbox
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
namespace RoslynSandbox
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

            [Test]
            public static void CallingToString()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var text = typeof(C).GetMethod(nameof(M)).Invoke(null, null).ToString();
        }

        public static int M() => 0;
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("_ = ")]
            [TestCase("var _ = ")]
            [TestCase("var __ = ")]
            public static void Discarded(string discard)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            _ = typeof(C).GetMethod(nameof(M)).Invoke(null, null).ToString();
        }

        public static int M() => 0;
    }
}".AssertReplace("_ = ", discard);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
