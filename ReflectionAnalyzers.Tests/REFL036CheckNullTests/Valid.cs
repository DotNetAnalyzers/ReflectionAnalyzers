namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL036CheckNull.Descriptor;

        [Test]
        public static void TypeGetTypeElvisAssembly()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""C"")?.Assembly;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void TypeGetTypeThrowOnErrorAssembly()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""C"", throwOnError: true).Assembly;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
