namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDoNotMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ConstructorInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL025ArgumentsDoNotMatchParameters;

            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })")]
            public static void SingleIntParameter(string call)
            {
                var code = @"
namespace N
{
    public class C
    {
        public C(int value)
        {
            var foo = (int)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }

        public static int M(int value) => value;
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })", call);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
