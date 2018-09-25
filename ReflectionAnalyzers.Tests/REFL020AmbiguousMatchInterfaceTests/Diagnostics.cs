namespace ReflectionAnalyzers.Tests.REFL020AmbiguousMatchInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL020AmbiguousMatchInterface.Descriptor);

        [TestCase("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(↓\"IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(IEnumerable<>).FullName)")]
        public void GetInterface(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Foo : IEnumerable<int>, IEnumerable<double>
    {
        private readonly List<int> ints = new List<int>();
        private readonly List<double> doubles = new List<double>();

        public Foo()
        {
            var type = typeof(Foo).GetInterface(↓""System.Collections.Generic.IEnumerable`1"");
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator<double> IEnumerable<double>.GetEnumerator() => this.doubles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }
}".AssertReplace("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
