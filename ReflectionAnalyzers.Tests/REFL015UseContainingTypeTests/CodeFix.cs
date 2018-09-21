namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseContainingTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL015");

        [TestCase("GetMember(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMember(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        public void GetPrivateMemberTypeof(string call)
        {
            var baseCode = @"
namespace RoslynSandbox
{
    using System;

    public class FooBase
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

    public class Foo : FooBase
    {
        public Foo()
        {
            var member = typeof(↓Foo).GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : FooBase
    {
        public Foo()
        {
            var member = typeof(FooBase).GetField(""PrivateStaticField"", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);
            var message = "Use the containing type FooBase.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { baseCode, code }, fixedCode);
        }

        [TestCase("PublicStatic")]
        [TestCase("Public")]
        public void GetPublicNestedType(string type)
        {
            var baseCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class FooBase
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

    public class Foo : FooBase
    {
        public Foo()
        {
            var typeInfo = typeof(↓Foo).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : FooBase
    {
        public Foo()
        {
            var typeInfo = typeof(FooBase).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})");
            var message = "Use the containing type FooBase.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { baseCode, code }, fixedCode);
        }

        [TestCase("PrivateStatic")]
        [TestCase("Private")]
        public void GetPrivateNestedType(string type)
        {
            var baseCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class FooBase
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

    public class Foo : FooBase
    {
        public Foo()
        {
            var typeInfo = typeof(↓Foo).GetNestedType(""PrivateStatic"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("PrivateStatic", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : FooBase
    {
        public Foo()
        {
            var typeInfo = typeof(FooBase).GetNestedType(""PrivateStatic"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("PrivateStatic", type);

            var message = "Use the containing type FooBase.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { baseCode, code }, fixedCode);
        }
    }
}
