namespace ReflectionAnalyzers.Tests.REFL022UseFullyQualifiedNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetInterfaceAnalyzer();
        private static readonly CodeFixProvider Fix = new UseFullyQualifiedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL022UseFullyQualifiedName);

        [TestCase("GetInterface(↓\"IEnumerable`1\")", "GetInterface(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("GetInterface(typeof(IEnumerable<>).↓Name)", "GetInterface(typeof(IEnumerable<>).FullName)")]
        [TestCase("GetInterface(↓\"IEnumerable\")", "GetInterface(\"System.Collections.IEnumerable\")")]
        [TestCase("GetInterface(typeof(IEnumerable).↓Name)", "GetInterface(typeof(IEnumerable).FullName)")]
        public static void GetInterface(string call, string expected)
        {
            var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class C : IEnumerable<int>
    {
        private readonly List<int> ints = new List<int>();

        public C()
        {
            var type = typeof(C).GetInterface(↓typeof(IEnumerable<>).Name);
        }

        public IEnumerator<int> GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}".AssertReplace("GetInterface(↓typeof(IEnumerable<>).Name)", call);

            var after = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class C : IEnumerable<int>
    {
        private readonly List<int> ints = new List<int>();

        public C()
        {
            var type = typeof(C).GetInterface(typeof(IEnumerable<>).FullName);
        }

        public IEnumerator<int> GetEnumerator() => this.ints.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}".AssertReplace("GetInterface(typeof(IEnumerable<>).FullName)", expected);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
