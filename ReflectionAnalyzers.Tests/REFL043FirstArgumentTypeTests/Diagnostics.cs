namespace ReflectionAnalyzers.Tests.REFL043FirstArgumentTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly CreateDelegateAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL043FirstArgumentType);

        [Test]
        public static void StaticStringVoidFirstArg()
        {
            var code = @"
namespace N
{
    using System;

    class C
    {
        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action),
            ↓new C(),
            typeof(C).GetMethod(nameof(M)));
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
