namespace ReflectionAnalyzers.Tests.REFL042FirstArgumentMustBeReferenceTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL042FirstArgumentIsReferenceType);

        [Test]
        public static void StaticStringVoidFirstArg()
        {
            var code = @"
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
