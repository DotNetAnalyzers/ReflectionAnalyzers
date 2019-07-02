namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class GetCustomAttribute
        {
            private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL044ExpectedAttributeType.DiagnosticId);

            [Test]
            public static void AttributeGetCustomAttribute()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => Attribute.GetCustomAttribute(typeof(C), ↓typeof(string)) == null;
    }
}";

                var message = "Expected attribute type.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
