namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class ConstructorInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })")]
            public void SingleIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int value)
        {
            var foo = (int)typeof(Foo).GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 });
        }

        public static int Bar(int value) => value;
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })", call);

                var message = "Use overload of Invoke without obj parameter.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
