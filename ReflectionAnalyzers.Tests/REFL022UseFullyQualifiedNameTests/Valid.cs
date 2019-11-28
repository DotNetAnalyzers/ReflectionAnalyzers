namespace ReflectionAnalyzers.Tests.REFL022UseFullyQualifiedNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL022UseFullyQualifiedName;

        [TestCase("GetInterface(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(IEnumerable).FullName)")]
        [TestCase("GetInterface(typeof(IEnumerable<>).FullName)")]
        [TestCase("GetInterface(\"System.Collections.IEnumerable\")")]
        public static void GetInterface(string call)
        {
            var code = @"
namespace N
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
