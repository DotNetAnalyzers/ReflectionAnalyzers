namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL012PreferIsDefined.Descriptor;

        [Test]
        public static void WhenUsingGeneric()
        {
            var code = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WhenGetCustomAttributeCast()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public C()
        {
            var attribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WhenGetCustomAttributeAs()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public C()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) as ObsoleteAttribute;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
