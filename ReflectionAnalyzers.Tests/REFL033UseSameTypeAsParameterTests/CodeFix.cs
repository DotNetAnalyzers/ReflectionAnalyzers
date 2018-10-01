namespace ReflectionAnalyzers.Tests.REFL033UseSameTypeAsParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseParameterTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL033UseSameTypeAsParameter.Descriptor);

        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)")]
        public void GetMethodOneParameterOverloadResolution(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)", call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null)", call.AssertReplace("↓int", "IComparable"));

            var message = "Use the same type as the parameter. Expected: IComparable.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void TwoProjects()
        {
            var fooCode = @"
namespace Project1
{
    using System;

    public class Foo
    {
        public static IComparable Static(IComparable i) => i;
    }
}";

            var code = @"
namespace Project2
{
    using System;
    using System.Reflection;

    using Project1;

    public class Bar
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Foo.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null);
    }
}";

            var fixedCode = @"
namespace Project2
{
    using System;
    using System.Reflection;

    using Project1;

    public class Bar
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Foo.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);
    }
}";
            var message = "Use the same type as the parameter. Expected: IComparable.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { fooCode, code }, fixedCode);
        }
    }
}
