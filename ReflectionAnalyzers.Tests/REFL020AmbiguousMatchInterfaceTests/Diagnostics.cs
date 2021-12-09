namespace ReflectionAnalyzers.Tests.REFL020AmbiguousMatchInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly GetInterfaceAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL020AmbiguousMatchInterface);

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

    public class C : IEnumerable<int>, IEnumerable<double>
    {
        private readonly List<int> ints = new List<int>();
        private readonly List<double> doubles = new List<double>();

        public C()
        {
            var type = typeof(C).GetInterface(↓""System.Collections.Generic.IEnumerable`1"");
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator<double> IEnumerable<double>.GetEnumerator() => this.doubles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }
}".AssertReplace("GetInterface(↓\"System.Collections.Generic.IEnumerable`1\")", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
