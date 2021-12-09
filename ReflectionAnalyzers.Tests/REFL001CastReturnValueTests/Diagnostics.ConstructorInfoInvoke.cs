namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class ConstructorInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

            [Test]
            public static void AssigningLocal()
            {
                var code = @"
namespace N
{
    public class C
    {
        public C(int i)
        {
            var value = ↓typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
