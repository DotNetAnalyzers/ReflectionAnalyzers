namespace ReflectionAnalyzers.Tests.REFL023TypeDoesNotImplementInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL023TypeDoesNotImplementInterface.Descriptor);

        [TestCase("GetInterface(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(\"IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(IEnumerable).FullName)")]
        [TestCase("GetInterface(typeof(IEnumerable).Name)")]
        [TestCase("GetInterface(typeof(IEnumerable<>).FullName)")]
        [TestCase("GetInterface(typeof(IEnumerable<>).Name)")]
        [TestCase("GetInterface(\"IEnumerable\")")]
        [TestCase("GetInterface(\"System.Collections.IEnumerable\")")]
        public void GetInterface(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections;
    using System.Collections.Generic;

    public class Foo : IEnumerable<int>
    {
        private readonly List<int> ints = new List<int>();

        public Foo()
        {
            var type = typeof(Foo).GetInterface(""System.Collections.Generic.IEnumerable`1"");
        }

        public IEnumerator<int> GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}".AssertReplace("GetInterface(\"System.Collections.Generic.IEnumerable`1\")", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
