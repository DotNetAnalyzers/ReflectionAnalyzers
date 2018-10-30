namespace ReflectionAnalyzers.Tests.REFL041CreateDelegateTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
        private static readonly CodeFixProvider Fix = new UseTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL041CreateDelegateType.Descriptor);

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void StaticStringInt(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static int M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static int M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string, int>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Func<string, int>.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void StaticVoid(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Action.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void StaticStringVoid(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void StaticStringVoidFirstArg(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<int>),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Action<int>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void StaticStringStringVoidFirstArg(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg1, string arg2) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<int>),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Action<int>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static void M(string arg1, string arg2) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<string>),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void InstanceStringInt(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public int M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public int M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<C, string, int>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Func<C, string, int>.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void InstanceVoid(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<C>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Action<C>.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public void InstanceVoidWithTarget(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<string>),
            new C(),
            typeof(C).GetMethod(nameof(M)));
    }
}".AssertReplace("typeof(Func<string>)", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public void M() { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action),
            new C(),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Action.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void StaticStringIntCustomDelegate()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        delegate int StringInt(string text);

        public static void M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(StringInt),
            typeof(C).GetMethod(nameof(M)));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        delegate int StringInt(string text);

        public static void M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Action<string>.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
