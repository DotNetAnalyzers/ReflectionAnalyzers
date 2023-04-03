namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDoNotMatchParametersTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class Diagnostics
{
    public static class ConstructorInfoInvoke
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL025ArgumentsDoNotMatchParameters);

        [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { ↓1.2 })")]
        [TestCase("GetConstructor(new[] { typeof(int) })!.Invoke(new object[] { ↓1.2 })")]
        [TestCase("GetConstructor(new[] { typeof(int) })?.Invoke(new object[] { ↓1.2 })")]
        public static void SingleIntParameter(string call)
        {
            var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public C(int value)
        {
            var foo = (int)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { ↓1.2 });
        }

        public static int M(int value) => value;
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { ↓1.2 })", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
