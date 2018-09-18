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
            var typeInfo = ↓typeof(Foo).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
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
            var typeInfo = ↓typeof(Foo).GetNestedType(""PrivateStatic"", BindingFlags.NonPublic);
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
