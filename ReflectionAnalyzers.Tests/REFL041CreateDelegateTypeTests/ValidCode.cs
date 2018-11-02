namespace ReflectionAnalyzers.Tests.REFL041CreateDelegateTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL041CreateDelegateType.Descriptor;

        [Test]
        public void CreateDelegateParameterExpressionMake()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    class C
    {

        public static object Get => Delegate.CreateDelegate(
            typeof(Func<Type, string, bool, ParameterExpression>),
            typeof(ParameterExpression).GetMethod(""Make"", BindingFlags.Static | BindingFlags.NonPublic));
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void StaticStringInt()
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
            typeof(Func<string, int>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void StaticStringIntWithFirstArg()
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
            typeof(Func<int>),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void StaticVoid()
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
            typeof(Action),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void StaticStringVoid()
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
            typeof(Action<string>),
            typeof(C).GetMethod(nameof(M)));
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void StaticStringVoidFirstArg()
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
            typeof(Action),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void StaticStringStringVoidFirstArg()
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
            typeof(Action<string>),
            string.Empty,
            typeof(C).GetMethod(nameof(M)));
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void InstanceStringInt()
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
            typeof(Func<C, string, int>),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void InstanceStringIntWithTarget()
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
            typeof(Func<string, int>),
            new C(),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
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

        public static int M(string arg) => arg.Length;

        public static object Get => Delegate.CreateDelegate(
            typeof(StringInt),
            typeof(C).GetMethod(nameof(M)));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void GetGetMethodReturnType()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        // incorrect return type.
        public static Func<Foo, int> Getter { get; } =
            (Func<Foo, int>)Delegate.CreateDelegate(
                typeof(Func<Foo, int>),
                typeof(Foo).GetProperty(
                    nameof(Foo.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

        public int Value { get; set; }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void GetMethodReturnType()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        // incorrect return type.
        public static Func<Foo, int> Getter { get; } =
            (Func<Foo, int>)Delegate.CreateDelegate(
                typeof(Func<Foo, int>),
                typeof(Foo).GetProperty(
                    nameof(Foo.Value),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod);

        public int Value { get; set; }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
