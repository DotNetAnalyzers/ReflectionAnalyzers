namespace ReflectionAnalyzers.Tests.REFL010PreferGenericGetCustomAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL010");

        [Test]
        public void WhenUsingGeneric()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var attribute = typeof(Foo).GetCustomAttribute<ObsoleteAttribute>();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void NoCastNoFix()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(Foo), typeof(ObsoleteAttribute));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
