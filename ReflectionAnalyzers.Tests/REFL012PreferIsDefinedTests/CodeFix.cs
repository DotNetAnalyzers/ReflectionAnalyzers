namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly CodeFixProvider Fix = new GetCustomAttributeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL012PreferIsDefined.DiagnosticId);

        [Test]
        public void AttributeGetCustomAttributeEqualsNull()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public static bool Bar() => ↓Attribute.GetCustomAttribute(typeof(Foo), typeof(ObsoleteAttribute)) == null;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public static bool Bar() => !Attribute.IsDefined(typeof(Foo), typeof(ObsoleteAttribute));
    }
}";
            var message = "Prefer Attribute.IsDefined().";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void AttributeGetCustomAttributeNotEqualsNull()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public static bool Bar() => ↓Attribute.GetCustomAttribute(typeof(Foo), typeof(ObsoleteAttribute)) != null;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public static bool Bar() => Attribute.IsDefined(typeof(Foo), typeof(ObsoleteAttribute));
    }
}";
            var message = "Prefer Attribute.IsDefined().";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
