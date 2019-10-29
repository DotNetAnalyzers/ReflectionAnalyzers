namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class IsDefined
        {
            private static readonly DiagnosticAnalyzer Analyzer = new IsDefinedAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL044ExpectedAttributeType.DiagnosticId);

            [Test]
            public static void AttributeIsDefined()
            {
                var code = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => Attribute.IsDefined(typeof(C), ↓typeof(string));
    }
}";

                var message = "Expected attribute type.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void IsDefinedExtensionMethod()
            {
                var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public static bool M() => typeof(C).IsDefined(↓typeof(string));
    }
}";

                var message = "Expected attribute type.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
