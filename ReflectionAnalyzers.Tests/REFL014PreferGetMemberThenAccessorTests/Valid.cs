namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL014PreferGetMemberThenAccessor.Descriptor;

        [Test]
        public static void GetPropertyGetMethod()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(this.Value)).GetMethod;
        }

        public int Value { get; set; }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void GetPropertySetMethod()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(this.Value)).SetMethod;
        }

        public int Value { get; set; }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
