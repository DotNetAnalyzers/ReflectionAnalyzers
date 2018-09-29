namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class Diagnostics
    {
        public class ConstructorInfoInvoke
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
        public Foo(int i)
        {
            var value = ↓typeof(Foo).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void Walk()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int i)
        {
            var info = typeof(Foo).GetConstructor(new[] { typeof(int) });
            var value = ↓info.Invoke(new object[] { 1 });
        }
    }
}";

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
