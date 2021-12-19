namespace ReflectionAnalyzers.Tests.REFL023TypeDoesNotImplementInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly GetInterfaceAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL023TypeDoesNotImplementInterface);

        [TestCase("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(↓\"IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(System.Collections.Generic.IEnumerable<>).FullName)")]
        public static void GetInterface(string call)
        {
            var code = @"
#pragma warning disable CS8604
namespace N
{
    public class C
    {
        public object? Get() => typeof(C).GetInterface(↓""System.Collections.Generic.IEnumerable`1"");
    }
}".AssertReplace("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
