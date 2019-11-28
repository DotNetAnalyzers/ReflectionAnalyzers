namespace ReflectionAnalyzers.Tests.REFL023TypeDoesNotImplementInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL023TypeDoesNotImplementInterface);

        [TestCase("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(↓\"IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(IEnumerable<>).FullName)")]
        public static void GetInterface(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var type = typeof(C).GetInterface(↓""System.Collections.Generic.IEnumerable`1"");
        }
    }
}".AssertReplace("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
