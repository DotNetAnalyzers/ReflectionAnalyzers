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

            [TestCase("Activator.CreateInstance(typeof(T))")]
            [TestCase("Activator.CreateInstance(typeof(T), true)")]
            [TestCase("Activator.CreateInstance(typeof(T), false)")]
            [TestCase("Activator.CreateInstance(typeof(T), \"foo\")")]
            public void WhenUnconstrainedGeneric(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
        {
            var foo = ↓Activator.CreateInstance(typeof(T));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(T))", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
