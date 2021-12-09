namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class GetCustomAttribute
        {
            private static readonly GetCustomAttributeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL044");

            [Test]
            public static void AttributeGetCustomAttribute()
            {
                var code = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => Attribute.GetCustomAttribute(typeof(C), ↓typeof(string)) == null;
    }
}";

                var message = "Expected attribute type.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
