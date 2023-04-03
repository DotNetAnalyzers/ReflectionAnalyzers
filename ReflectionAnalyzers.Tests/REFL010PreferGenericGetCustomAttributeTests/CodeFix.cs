namespace ReflectionAnalyzers.Tests.REFL010PreferGenericGetCustomAttributeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly GetCustomAttributeAnalyzer Analyzer = new();
    private static readonly UseGenericGetCustomAttributeFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL010");

    [Test]
    public static void WhenCast()
    {
        var before = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var attribute = (ObsoleteAttribute)↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute));
        }
    }
}";
        var after = @"
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
        var message = "Prefer the generic extension method GetCustomAttribute<ObsoleteAttribute>";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
    }

    [Test]
    public static void WhenAs()
    {
        var before = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var attribute = ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) as ObsoleteAttribute;
        }
    }
}";
        var after = @"
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
