namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDoNotMatchParametersTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static partial class Valid
{
    public static class ActivatorCreateInstance
    {
        private static readonly ActivatorAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL025ArgumentsDoNotMatchParameters;

        [TestCase("Activator.CreateInstance(typeof(C))")]
        [TestCase("Activator.CreateInstance(this.GetType())")]
        [TestCase("Activator.CreateInstance<C>()")]
        public static void ExplicitDefaultConstructor(string call)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var c = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("(C)Activator.CreateInstance(typeof(C))")]
        [TestCase("Activator.CreateInstance<C>()")]
        public static void ImplicitDefaultConstructor(string call)
        {
            var code = @"
#nullable disable
namespace N
{
    using System;

    public class C
    {
        public static C Create() => Activator.CreateInstance<C>();
    }
}".AssertReplace("Activator.CreateInstance<C>()", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), 1)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        public static void OneConstructorOneIntParameter(string call)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i)
        {
            var c = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), new object[] { System.Reflection.Missing.Value })")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        public static void OneConstructorOptionalIntParameter(string call)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i = 0)
        {
            var c = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
        [TestCase("Activator.CreateInstance(typeof(C), new[] { (object)null })")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { null })")]
        [TestCase("Activator.CreateInstance(typeof(C), (string)null)")]
        public static void OneConstructorSingleStringParameter(string call)
        {
            var code = @"
#nullable disable
namespace N
{
    using System;

    public class C
    {
        public C(string text)
        {
            var c = Activator.CreateInstance(typeof(C), ""abc"");
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), 1)")]
        [TestCase("Activator.CreateInstance(typeof(C), 1.2)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1.2 })")]
        public static void OneConstructorOneDoubleParameter(string call)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(double d)
        {
            var c = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
        public static void OverloadedConstructorsStringAndStringBuilder(string call)
        {
            var code = @"
#nullable disable
namespace N
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { \"abc\" })")]
        [TestCase("Activator.CreateInstance(typeof(C), 1)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        public static void OverloadedConstructorsStringAndInt(string call)
        {
            var code = @"
#nullable disable
namespace N
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
        [TestCase("Activator.CreateInstance(typeof(C), (string)null)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { null })")]
        [TestCase("Activator.CreateInstance(typeof(C), \"abc\", \"cde\")")]
        public static void OverloadedConstructorsDifferentLength(string call)
        {
            var code = @"
#nullable disable
namespace N
{
    using System;

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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C))")]
        [TestCase("Activator.CreateInstance(typeof(C), 1)")]
        [TestCase("Activator.CreateInstance(typeof(C), null)")]
        [TestCase("Activator.CreateInstance(typeof(C), 1, 2)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1, 2 })")]
        public static void ParamsConstructor(string call)
        {
            var code = @"
#nullable disable
namespace N
{
    using System;

    public class C
    {
        public C(params int[] ints)
        {
            var c = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("Activator.CreateInstance(typeof(C), 1)")]
        [TestCase("Activator.CreateInstance(typeof(C), 1, 2)")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
        [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1, 2 })")]
        [TestCase("Activator.CreateInstance(typeof(C), 1, new[] { 2, 3 })")]
        [TestCase("Activator.CreateInstance(typeof(C), 1, new int[0])")]
        [TestCase("Activator.CreateInstance(typeof(C), 1, Array.Empty<int>())")]
        public static void ParamsConstructorSecondParameter(string call)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i, params int[] ints)
        {
            var c = Activator.CreateInstance(typeof(C), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WhenUnknown()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? M(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WhenUnconstrainedGeneric()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? M<T>() => Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Issue202()
        {
            var code = @"
#nullable disable
namespace N
{
    using System;

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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
