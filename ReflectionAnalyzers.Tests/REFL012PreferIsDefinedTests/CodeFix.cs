﻿namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseIsDefinedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL012PreferIsDefined);

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
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
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
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
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
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
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
