namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MakeGenericMethod
        {
            private static readonly MakeGenericAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL034DoNotMakeGeneric);

            [Test]
            public static void WhenNotGeneric()
            {
                var code = @"
namespace N
{
    public class C
    {
        public static object M() => typeof(C).GetMethod(nameof(M)).↓MakeGenericMethod(typeof(int));
    }
}";
                var message = "N.C.M() is not a GenericMethodDefinition. MakeGenericMethod may only be called on a method for which MethodBase.IsGenericMethodDefinition is true.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
