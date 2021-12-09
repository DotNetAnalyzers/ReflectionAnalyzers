namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class MethodInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly CastReturnValueFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

            [Test]
            public static void AssigningLocal()
            {
                var before = @"
namespace N
{
    public class C
    {
        public C()
        {
            var value = ↓typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
