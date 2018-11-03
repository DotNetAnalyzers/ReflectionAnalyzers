namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL044ExpectedAttributeType.DiagnosticId);

        [Test]
        public void AttributeGetCustomAttribute()
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
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
