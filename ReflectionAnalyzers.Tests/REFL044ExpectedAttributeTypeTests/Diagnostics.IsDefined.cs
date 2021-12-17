namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class IsDefined
        {
            private static readonly IsDefinedAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL044");

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

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
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

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
