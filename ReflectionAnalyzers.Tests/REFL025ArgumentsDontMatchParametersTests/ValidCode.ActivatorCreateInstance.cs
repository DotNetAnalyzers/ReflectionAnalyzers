namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL025ArgumentsDontMatchParameters.Descriptor;

            [TestCase("Activator.CreateInstance(typeof(C))")]
            [TestCase("Activator.CreateInstance(this.GetType())")]
            [TestCase("Activator.CreateInstance<C>()")]
            public void ExplicitDefaultConstructor(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var foo = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("(C)Activator.CreateInstance(typeof(C))")]
            [TestCase("Activator.CreateInstance<C>()")]
            public void ImplicitDefaultConstructor(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static C Create() => Activator.CreateInstance<C>();
    }
}".AssertReplace("Activator.CreateInstance<C>()", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), 1)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            public void OneConstructorOneIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i)
        {
            var foo = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), new object[] { Missing.Value })")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            public void OneConstructorOptionalIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        public C(int i = 0)
        {
            var foo = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), new[] { (object)null })")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { null })")]
            [TestCase("Activator.CreateInstance(typeof(C), (string)null)")]
            public void OneConstructorSingleStringParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(string text)
        {
            var foo = Activator.CreateInstance(typeof(C), ""abc"");
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), 1)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1.2)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1.2 })")]
            public void OneConstructorOneDoubleParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(double d)
        {
            var foo = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
            public void OverloadedConstructorsStringAndStringBuilder(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Text;

    public class C
    {
        public C(string text)
        {
        }

        public C(StringBuilder text)
        {
        }

        public static C Create() => (C)Activator.CreateInstance(typeof(C), ""abc"");
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { \"abc\" })")]
            [TestCase("Activator.CreateInstance(typeof(C), 1)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            public void OverloadedConstructorsStringAndInt(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(string text)
        {
        }

        public C(int value)
        {
        }

        public static C Create() => (C)Activator.CreateInstance(typeof(C), ""abc"");
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), (string)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { null })")]
            [TestCase("Activator.CreateInstance(typeof(C), \"abc\", \"cde\")")]
            public void OverloadedConstructorsDifferentLength(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Text;

    public class C
    {
        public C(string text)
        {
        }

        public C(string text1, string text2)
        {
        }

        public static C Create() => (C)Activator.CreateInstance(typeof(C), ""abc"");
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C))")]
            [TestCase("Activator.CreateInstance(typeof(C), 1)")]
            [TestCase("Activator.CreateInstance(typeof(C), null)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, 2)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1, 2 })")]
            public void ParamsConstructor(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(params int[] ints)
        {
            var foo = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), 1)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, 2)")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1, 2 })")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, new[] { 2, 3 })")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, new int[0])")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, Array.Empty<int>())")]
            public void ParamsConstructorSecondParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i, params int[] ints)
        {
            var foo = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void WhenUnknown()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Bar(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void WhenUnconstrainedGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Bar<T>() => Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void Issue202()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    internal static class C
    {
        internal static object Create<T1, T2>()
        {
            if (!typeof(T1).IsValueType &&
                !typeof(T2).IsValueType)
            {
                return Activator.CreateInstance(typeof(C<,>).MakeGenericType(typeof(T1), typeof(T2)));
            }

            return null;
        }
    }

    internal class C<T1, T2>
        where T1 : class
        where T2 : class
    {
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
