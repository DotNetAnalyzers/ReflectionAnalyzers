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

            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, new object[] { ↓1.2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, new object[] { ↓\"abc\" })")]
            public void SingleIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 });
        }

        public static int Bar(int value) => value;
    }
}".AssertReplace("GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 })", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1.2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { \"abc\" })")]
            public void NoParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 });
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 })", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
