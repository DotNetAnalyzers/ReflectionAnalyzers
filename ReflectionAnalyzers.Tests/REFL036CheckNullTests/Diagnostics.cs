namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL036CheckNull.Descriptor);

        [Test]
        public void TypeGetTypeAssembly()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""Foo"").Assembly;
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void TypeGetTypeThrowOnErrorAssembly()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""Foo"", throwOnError: false).Assembly;
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
