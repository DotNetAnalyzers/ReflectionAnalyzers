namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor);

            [TestCase("Invoke(null, ↓new object[] { 1.2 })")]
            [TestCase("Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("Invoke(null, ↓new object[] { \"abc\" })")]
            public void SingleIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var foo = typeof(Foo).GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1.2 });
        }

        public int Bar(int value) => value;
    }
}".AssertReplace("Invoke(null, ↓new object[] { 1.2 })", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
