namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        public class MakeGenericMethod
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public void SingleUnconstrained()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
        {
            var method = typeof(Foo).GetMethod(nameof(Foo.Bar), Type.EmptyTypes).MakeGenericMethod(typeof(int));
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
