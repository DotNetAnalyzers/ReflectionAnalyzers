namespace ReflectionAnalyzers.Tests.REFL012PreferIsDefinedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetCustomAttributeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseIsDefinedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL012PreferIsDefined.DiagnosticId);

        [Test]
        public static void AttributeGetCustomAttributeEqualsNull()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) == null;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => !Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute));
    }
}";
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public static void AttributeGetCustomAttributeEqualsNullExplicitInherit()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute), true) == null;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => !Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute), true);
    }
}";
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public static void AttributeGetCustomAttributeNotEqualsNull()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => ↓Attribute.GetCustomAttribute(typeof(C), typeof(ObsoleteAttribute)) != null;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public static bool Bar() => Attribute.IsDefined(typeof(C), typeof(ObsoleteAttribute));
    }
}";
            var message = "Prefer Attribute.IsDefined().";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase(" == null")]
        [TestCase(" is null")]
        public static void IfGetCustomAttributeIsNull(string isNull)
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
            if (typeof(C).GetCustomAttribute(typeof(ObsoleteAttribute)) == null)
            {
            }
        }
    }
}".AssertReplace(" == null", isNull);
            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void IfGetCustomAttributeNotNull()
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
            if (typeof(C).GetCustomAttribute(typeof(ObsoleteAttribute)) != null)
            {
            }
        }
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
