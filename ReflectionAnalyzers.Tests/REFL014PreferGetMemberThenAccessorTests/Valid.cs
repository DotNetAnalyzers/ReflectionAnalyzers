namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL014PreferGetMemberThenAccessor;

        [TestCase("typeof(C).GetProperty(nameof(this.Value)).GetMethod")]
        [TestCase("typeof(C).GetProperty(nameof(this.Value))!.GetMethod")]
        [TestCase("typeof(C).GetProperty(nameof(this.Value))?.GetMethod")]
        public static void GetPropertyGetMethod(string expression)
        {
            var code = @"
#pragma warning disable CS8602
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
}".AssertReplace("typeof(C).GetProperty(nameof(this.Value)).GetMethod", expression);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(C).GetProperty(nameof(this.Value)).SetMethod")]
        [TestCase("typeof(C).GetProperty(nameof(this.Value))!.SetMethod")]
        [TestCase("typeof(C).GetProperty(nameof(this.Value))?.SetMethod")]
        public static void GetPropertySetMethod(string expression)
        {
            var code = @"
#pragma warning disable CS8602
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
}".AssertReplace("typeof(C).GetProperty(nameof(this.Value)).SetMethod", expression);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
