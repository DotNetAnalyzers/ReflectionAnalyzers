namespace ReflectionAnalyzers.Tests.REFL041CreateDelegateTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
        private static readonly CodeFixProvider Fix = new UseTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL041CreateDelegateType.Descriptor);

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void StaticStringInt(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void StaticVoid(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void StaticStringVoid(string type)
        {
            var before = @"
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

            var after = @"
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void StaticStringVoidFirstArg(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void StaticStringStringVoidFirstArg(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void InstanceStringInt(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void InstanceVoid(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("typeof(Func<string>)")]
        [TestCase("typeof(Func<string, string>)")]
        [TestCase("typeof(Func<string, string, int>)")]
        [TestCase("typeof(Action<int>)")]
        [TestCase("typeof(Action<string, string>)")]
        public static void InstanceVoidWithTarget(string type)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void StaticStringIntCustomDelegate()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        delegate int StringInt(string text);

        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(StringInt),
            typeof(C).GetMethod(nameof(M)));
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        delegate int StringInt(string text);

        public static void M(string arg) { }

        public static object Get => Delegate.CreateDelegate(
            typeof(Action<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            var message = "Delegate type is not matching expected System.Action<string>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetGetMethodReturnTypeInstance()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<C, string>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

        public int Value { get; set; }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<C, int>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

        public int Value { get; set; }
    }
}";
            var message = "Delegate type is not matching expected System.Func<C, int>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetGetMethodReturnTypeStatic()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<string>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetGetMethod());

        public static int Value { get; set; }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetGetMethod());

        public static int Value { get; set; }
    }
}";
            var message = "Delegate type is not matching expected System.Func<int>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetSetMethodReturnTypeInstance()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Action<C, string>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetSetMethod());

        public int Value { get; set; }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Action<C, int>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetSetMethod());

        public int Value { get; set; }
    }
}";
            var message = "Delegate type is not matching expected System.Action<C, int>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetSetMethodReturnTypeStatic()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Action<string>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetSetMethod());

        public static int Value { get; set; }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Action<int>),
                typeof(C).GetProperty(
                    nameof(C.Value),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetSetMethod());

        public static int Value { get; set; }
    }
}";
            var message = "Delegate type is not matching expected System.Action<int>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void StaticWithContainingAsArgument()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<C, string>),
                typeof(C).GetMethod(nameof(C.M)));

        public int P { get; set; }

        public static int M(C c) => c.P;
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Getter { get; } = Delegate.CreateDelegate(
                typeof(Func<C, int>),
                typeof(C).GetMethod(nameof(C.M)));

        public int P { get; set; }

        public static int M(C c) => c.P;
    }
}";
            var message = "Delegate type is not matching expected System.Func<C, int>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }
    }
}
