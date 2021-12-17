namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly GetCustomAttributeAnalyzer Analyzer = new();
        private static readonly UseIsDefinedFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL012PreferIsDefined);

        [Test]
        public static void Message()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) == null;
    }
}";
            var after = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => !Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute));
    }
}";
            var message = "Prefer Attribute.IsDefined()";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void AttributeGetCustomAttributeEqualsNull()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) == null;
    }
}";
            var after = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => !Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AttributeGetCustomAttributeEqualsNullExplicitInherit()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute), true) == null;
    }
}";
            var after = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => !Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute), true);
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AttributeGetCustomAttributeNotEqualsNull()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) != null;
    }
}";
            var after = @"
namespace N
{
    using System;

    class C
    {
        public static bool M() => Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase(" == null")]
        [TestCase(" is null")]
        public static void IfGetCustomAttributeIsNull(string isNull)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            if (typeof(C).GetCustomAttribute(typeof(ObsoleteAttribute)) == null)
            {
            }
        }
    }
}".AssertReplace(" == null", isNull);
            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            if (!typeof(C).IsDefined(typeof(ObsoleteAttribute)))
            {
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void IfGetCustomAttributeNotNull()
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            if (↓typeof(C).GetCustomAttribute(typeof(ObsoleteAttribute)) != null)
            {
            }
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
            if (typeof(C).IsDefined(typeof(ObsoleteAttribute)))
            {
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
