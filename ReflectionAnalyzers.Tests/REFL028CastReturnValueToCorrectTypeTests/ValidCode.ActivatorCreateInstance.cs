namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL028CastReturnValueToCorrectType.Descriptor);

            [TestCase("(Foo)")]
            [TestCase("(IDisposable)")]
            public void WhenCasting(string cast)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        public Foo()
        {
            var foo = (Foo)Activator.CreateInstance(typeof(Foo));
        }

        public void Dispose()
        {
        }
    }
}".AssertReplace("(Foo)", cast);
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
