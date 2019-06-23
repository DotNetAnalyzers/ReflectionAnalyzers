namespace ReflectionAnalyzers.Tests.REFL020AmbiguousMatchInterfaceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL020AmbiguousMatchInterface.Descriptor;

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

    public class C : IEnumerable<int>
    {
        private readonly List<int> ints = new List<int>();

        public C()
        {
            var type = typeof(C).GetInterface(""System.Collections.Generic.IEnumerable`1"");
        }

        public IEnumerator<int> GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}".AssertReplace("GetInterface(\"System.Collections.Generic.IEnumerable`1\")", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
