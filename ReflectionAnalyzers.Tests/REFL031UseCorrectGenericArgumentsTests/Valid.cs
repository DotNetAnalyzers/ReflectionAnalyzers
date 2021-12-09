namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class MakeGenericMethod
        {
            private static readonly MakeGenericAnalyzer Analyzer = new();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL031UseCorrectGenericArguments;

            [Test]
            public static void SingleUnconstrained()
            {
                var code = @"
namespace N
{
    public class C
    {
        public static void M<T>()
        {
            var method = typeof(C).GetMethod(nameof(C.M)).MakeGenericMethod(typeof(int));
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : class",            "typeof(string)")]
            [TestCase("where T : struct",           "typeof(int)")]
            [TestCase("where T : IComparable",      "typeof(int)")]
            [TestCase("where T : IComparable<T>",   "typeof(int)")]
            [TestCase("where T : IComparable<int>", "typeof(int)")]
            [TestCase("where T : new()",            "typeof(C)")]
            public static void ConstrainedParameter(string constraint, string arg)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public static void M<T>()
            where T : class
        {
        }

        public object Get(Type unused) => typeof(C).GetMethod(nameof(C.M)).MakeGenericMethod(typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("typeof(T).IsValueType")]
            [TestCase("!!typeof(T).IsValueType")]
            [TestCase("typeof(T).IsValueType && true")]
            [TestCase("typeof(T).IsValueType && typeof(T).IsValueType")]
            public static void Ternary(string condition)
            {
                var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public MethodInfo Get<T>()
        {
            return typeof(T).IsValueType
                ? typeof(C).GetMethod(nameof(this.ConstrainedToStruct), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(typeof(T))
                : typeof(C).GetMethod(nameof(this.ConstrainedToClass), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(typeof(T));
        }

        public T ConstrainedToClass<T>(T t)
            where T : class
        {
            return t;
        }

        public T ConstrainedToStruct<T>(T t)
            where T : struct
        {
            return t;
        }
    }
}".AssertReplace("typeof(T).IsValueType", condition);
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void UnknownTypeGetGenericArguments()
            {
                var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;

    public class C
    {
        object M(Type t) => typeof(IDictionary<,>).MakeGenericType(t.GetGenericArguments());
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
