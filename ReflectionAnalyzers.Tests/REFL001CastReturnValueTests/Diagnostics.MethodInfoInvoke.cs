namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MethodInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

            [TestCase("typeof(C).GetMethod(nameof(M)).Invoke(null, null)")]
            [TestCase("typeof(C).GetMethod(nameof(M))!.Invoke(null, null)")]
            [TestCase("typeof(C).GetMethod(nameof(M))?.Invoke(null, null)")]
            public static void Simple(string expression)
            {
                var code = @"
#pragma warning disable CS8602
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
}".AssertReplace("typeof(C).GetMethod(nameof(M)).Invoke(null, null)", expression);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void Walk()
            {
                var code = @"
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = ↓info.Invoke(null, null);
        }

        public static int M() => 0;
    }
}";

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
