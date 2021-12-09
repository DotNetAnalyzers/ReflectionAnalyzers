namespace ReflectionAnalyzers.Tests.REFL010PreferGenericGetCustomAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetCustomAttributeAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL010PreferGenericGetCustomAttribute;

        [Test]
        public static void WhenUsingGeneric()
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var attribute = typeof(C).GetCustomAttribute<ObsoleteAttribute>();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void NoCastNoFix()
        {
            var code = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
