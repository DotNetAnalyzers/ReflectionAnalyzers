namespace ReflectionAnalyzers.Tests.REFL044ExpectedAttributeTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class GetCustomAttribute
        {
            private static readonly GetCustomAttributeAnalyzer Analyzer = new();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL044ExpectedAttributeType;

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
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void WhenGetCustomAttributeCast()
            {
                var code = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var attribute = (ObsoleteAttribute?)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute));
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void WhenGetCustomAttributeAs()
            {
                var code = @"
namespace N
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
}
