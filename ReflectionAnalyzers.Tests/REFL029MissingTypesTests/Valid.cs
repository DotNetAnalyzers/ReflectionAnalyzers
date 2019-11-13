namespace ReflectionAnalyzers.Tests.REFL029MissingTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL029MissingTypes.Descriptor;

        [Test]
        public static void GetMethodNoParameter()
        {
            var code = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), Type.EmptyTypes);
        }

        public int M() => 0;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void GetMethodOneParameter()
        {
            var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Id), new[] { typeof(int) });
        }

        public int Id(int value) => value;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void GetMethodOneGenericParameter()
        {
            var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Id));
        }

        public T Id<T>(T value) => value;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
