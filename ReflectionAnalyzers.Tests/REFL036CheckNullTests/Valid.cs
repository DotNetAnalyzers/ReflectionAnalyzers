namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetTypeAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL036CheckNull;

        [Test]
        public static void TypeGetTypeElvisAssembly()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? Get => Type.GetType(""C"")?.Assembly;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void TypeGetTypeThrowOnErrorAssembly()
        {
            var code = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public class C
    {
        public static object? Get => Type.GetType(""C"", throwOnError: true).Assembly;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
