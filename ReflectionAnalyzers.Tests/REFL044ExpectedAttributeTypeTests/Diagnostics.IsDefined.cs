namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class IsDefined
        {
            private static readonly DiagnosticAnalyzer Analyzer = new IsDefinedAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL044ExpectedAttributeType.DiagnosticId);

            [Test]
            public void AttributeIsDefined()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool M() => Attribute.IsDefined(typeof(C), ↓typeof(string));
    }
}";

                var message = "Expected attribute type.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public void IsDefinedExtensionMethod()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public static bool M() => typeof(C).IsDefined(↓typeof(string));
    }
}";

                var message = "Expected attribute type.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
