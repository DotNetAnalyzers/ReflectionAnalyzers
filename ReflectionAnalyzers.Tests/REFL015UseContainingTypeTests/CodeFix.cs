namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseContainingTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL015");

        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        public static void GetPrivateMemberTypeof(string call)
        {
            var before = @"
namespace RoslynSandbox
{
    using System;

    public class CBase
    {
        private static int PrivateStaticField;

        private static event EventHandler PrivateStaticEvent;

        private static int PrivateStaticProperty { get; set; }

        private static int PrivateStaticMethod() => 0;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var member = typeof(↓C).GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);

            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var member = typeof(CBase).GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);
            var message = "Use the containing type CBase.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { before, code }, after);
        }

        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        public static void GetPrivateMemberThisGetType(string call)
        {
            var before = @"
namespace RoslynSandbox
{
    using System;

    public class CBase
    {
        private static int PrivateStaticField;

        private static event EventHandler PrivateStaticEvent;

        private static int PrivateStaticProperty { get; set; }

        private static int PrivateStaticMethod() => 0;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var member = ↓this.GetType().GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);

            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var member = typeof(CBase).GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);
            var message = "Use the containing type CBase.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { before, code }, after);
        }

        [TestCase("PublicStatic")]
        [TestCase("Public")]
        public static void GetPublicNestedType(string type)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class CBase
    {
        public static class PublicStatic
        {
        }

        public class Public
        {
        }
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var typeInfo = typeof(↓C).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})");

            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var typeInfo = typeof(CBase).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})");
            var message = "Use the containing type CBase.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { before, code }, after);
        }

        [TestCase("PrivateStatic")]
        [TestCase("Private")]
        public static void GetPrivateNestedType(string type)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class CBase
    {
        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var typeInfo = typeof(↓C).GetNestedType(""PrivateStatic"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("PrivateStatic", type);

            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            var typeInfo = typeof(CBase).GetNestedType(""PrivateStatic"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("PrivateStatic", type);

            var message = "Use the containing type CBase.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { before, code }, after);
        }

        [Test]
        public static void PrivateFieldInBase()
        {
            var baseCode = @"
namespace RoslynSandbox
{
    class B
    {
        private readonly int field;
    }
}";
            var before = @"
namespace RoslynSandbox
{
    using System.Reflection;

     class C : B
    {
        public object Get => typeof(↓C).GetField(""field"", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.Reflection;

     class C : B
    {
        public object Get => typeof(B).GetField(""field"", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}";

            var message = "Use the containing type B.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { baseCode, before }, after);
        }
    }
}
