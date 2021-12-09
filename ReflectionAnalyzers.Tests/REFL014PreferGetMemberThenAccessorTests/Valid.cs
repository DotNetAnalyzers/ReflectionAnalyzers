namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL014PreferGetMemberThenAccessor;

        [Test]
        public static void GetPropertyGetMethod()
        {
            var code = @"
namespace N
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
namespace N
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
