namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class Diagnostics
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

            [Test]
            public void Typeof()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar()
        {
            var foo = ↓Activator.CreateInstance(typeof(Foo));
        }
    }
}";

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void WhenUnconstrainedGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
        {
            var foo = ↓Activator.CreateInstance(typeof(T), ""foo"");
        }
    }
}";

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
