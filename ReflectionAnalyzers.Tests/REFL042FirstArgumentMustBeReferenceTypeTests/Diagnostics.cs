namespace ReflectionAnalyzers.Tests.REFL042FirstArgumentMustBeReferenceTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly CreateDelegateAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL042FirstArgumentIsReferenceType);

        [Test]
        public static void StaticStringVoidFirstArg()
        {
            var code = @"
#pragma warning disable CS8604
namespace N
{
    using System;

    class C
    {
        public static void M(int arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action),
            ↓1,
            typeof(C).GetMethod(nameof(M)));
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
