namespace ReflectionAnalyzers.Tests.REFL033UseMoreSpecificTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL033UseMoreSpecificTypes.Descriptor);

        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)")]
        public void GetMethodOneParameterOverloadResolution(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
