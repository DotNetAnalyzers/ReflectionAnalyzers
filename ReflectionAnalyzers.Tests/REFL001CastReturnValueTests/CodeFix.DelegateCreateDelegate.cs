namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class DelegateCreateDelegate
        {
            private static readonly CreateDelegateAnalyzer Analyzer = new();
            private static readonly CastReturnValueFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

            [Test]
            public static void AssigningLocal()
            {
                var before = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var value = ↓Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}";

                var after = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void Walk()
            {
                var before = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = ↓Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
        }

        public static int M() => 0;
    }
}";

                var after = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
        }

        public static int M() => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
