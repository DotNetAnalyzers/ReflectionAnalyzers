namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class GetAccessor
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly DiagnosticAnalyzer Analyzer = new GetAccessorAnalyzer();

            [TestCase("typeof(C).GetProperty(nameof(P)).↓GetMethod")]
            [TestCase("typeof(C).GetProperty(nameof(this.P)).↓GetMethod")]
            [TestCase("typeof(C).GetProperty(nameof(P)).↓GetGetMethod(true)")]
            public static void MissingGetter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        private int p;

        public object Get => typeof(C).GetProperty(nameof(this.P)).↓GetMethod;

        public int P
        {
            set => this.p = value;
        }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(this.P)).↓GetMethod", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("typeof(C).GetProperty(nameof(P)).↓SetMethod")]
            [TestCase("typeof(C).GetProperty(nameof(P)).↓GetSetMethod(true)")]
            public static void MissingSetter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).GetProperty(nameof(P)).↓SetMethod;


        public int P { get; }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(P)).↓SetMethod", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
