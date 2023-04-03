namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly GetCustomAttributeAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL012PreferIsDefined;

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

    [TestCase("(ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute))")]
    [TestCase("(ObsoleteAttribute?)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute))")]
    [TestCase("(ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute))!")]
    public static void WhenGetCustomAttributeCast(string expression)
    {
        var code = @"
#pragma warning disable CS8600
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var attribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute));
        }
    }
}".AssertReplace("(ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute))", expression);
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
