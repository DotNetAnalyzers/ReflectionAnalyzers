namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

        [Test]
        public void Static()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void Instance()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(new Foo(), null);
        }

        public int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
