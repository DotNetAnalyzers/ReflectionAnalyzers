namespace ReflectionAnalyzers.Tests.REFL025ActivatorCreateInstanceArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ActivatorCreateInstanceArguments.Descriptor);

        [TestCase("Activator.CreateInstance(typeof(Foo), ↓new object[] { 1, 2 })")]
        [TestCase("Activator.CreateInstance(typeof(Foo), ↓\"abc\")")]
        [TestCase("Activator.CreateInstance(typeof(Foo), ↓1.0)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), ↓1, 2)")]
        public void OneConstructorSingleIntParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(int i)
        {
            var foo = Activator.CreateInstance(typeof(Foo));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo))", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
