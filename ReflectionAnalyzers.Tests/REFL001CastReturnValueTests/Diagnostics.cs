namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

        [Test]
        public void AssigningLocal()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = ↓typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
