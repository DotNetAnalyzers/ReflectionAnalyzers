namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class DelegateCreateDelegate
        {
            private static readonly CreateDelegateAnalyzer Analyzer = new();
            private static readonly CastReturnValueFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL028CastReturnValueToCorrectType);

            [Test]
            public static void WhenCastingToWrongType()
            {
                var before = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var value = (↓Func<double>)Delegate.CreateDelegate(
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
        }
    }
}
