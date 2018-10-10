namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL036CheckNull.Descriptor;

        [Test]
        public void TypeGetTypeElvisAssembly()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""Foo"")?.Assembly;
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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
        public static object Get => Type.GetType(""Foo"", throwOnError: true).Assembly;
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
