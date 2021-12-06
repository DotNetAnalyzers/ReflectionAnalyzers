namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL031UseCorrectGenericArguments;

            [TestCase("string")]
            [TestCase("int")]
            [TestCase("int?")]
            [TestCase("EventArgs")]
            public static void SingleUnconstrained(string type)
            {
                var code = @"
namespace N
{
    using System;

    public class C<T>
    {
        public static void Get(Type unused) => typeof(C<>).MakeGenericType(typeof(int));
    }
}".AssertReplace("int", type);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : class", "typeof(string)")]
            [TestCase("where T : class", "typeof(Console)")]
            [TestCase("where T : struct", "typeof(int)")]
            [TestCase("where T : unmanaged", "typeof(int)")]
            [TestCase("where T : IComparable", "typeof(int)")]
            [TestCase("where T : IComparable<T>", "typeof(int)")]
            [TestCase("where T : new()", "typeof(C<int>)")]
            public static void ConstrainedParameter(string constraint, string arg)
            {
                var code = @"
namespace N
{
    using System;

    public class C<T>
        where T : class
    {
        public static object Get(Type unused) => typeof(C<>).MakeGenericType(typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T1 : class", "where T2 : T1", "typeof(object), typeof(int)")]
            public static void TransitiveConstraints(string where1, string where2, string types)
            {
                var code = @"
namespace N
{
    public class C<T1, T2> 
        where T1 : class
        where T2 : T1
    {
        public static object Get => typeof(C<,>).MakeGenericType(typeof(object), typeof(int));
    }
}".AssertReplace("where T1 : class", where1)
  .AssertReplace("where T2 : T1", where2)
  .AssertReplace("typeof(object), typeof(int)", types);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void ImplicitDefaultConstructor()
            {
                var code = @"
namespace N
{
    public struct S
    {
        public S(int param) { }
    }

    public class C<T> 
        where T : new()
    {
        public static object Get => typeof(C<>).MakeGenericType(typeof(S));
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : Enum", "AttributeTargets")]
            [TestCase("where T : struct, System.Enum", "AttributeTargets")]
            [TestCase("where T : Enum", "Enum")]
            [TestCase("where T : unmanaged", "int")]
            [TestCase("where T : unmanaged", "Safe")]
            [TestCase("where T : unmanaged", "AttributeTargets")]
            public static void ConstrainedToEnum(string constraint, string arg)
            {
                var safeCode = @"
namespace N
{
    using System;

    public struct Safe
    {
        public int Value1;
        public AttributeTargets Value2;
    }
}";

                var code = @"
namespace N
{
    using System;

    public class C<T> 
        where T : Enum
    {
        public static object Get(Type unused) => typeof(C<>).MakeGenericType(typeof(AttributeTargets));
    }
}".AssertReplace("where T : Enum", constraint)
  .AssertReplace("AttributeTargets", arg);

                RoslynAssert.Valid(Analyzer, Descriptor, safeCode, code);
            }

            [Test]
            public static void NestedType()
            {
                var code = @"
namespace N
{
    public class C
    {
        public static void M1()
        {
            var type = typeof(C).GetNestedType(""M2`1"").MakeGenericType(typeof(int));
        }

        public class M2<T>
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void NestedGenericInGeneric()
            {
                var code = @"
namespace N
{
    public class C<T>
    {
        public object Get => typeof(C<>.D<>).MakeGenericType(typeof(int), typeof(int));

        public class D<U>
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void PassingArrayOfUnknownToMakeGenericType()
            {
                var code = @"
namespace N
{
    using System;

    class C<T1, T2>
    {
        void M(Type[] types)
        {
            typeof(C<,>).MakeGenericType(types);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void Ternary()
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public Type Get<T>()
        {
            return typeof(T).IsValueType
                ? typeof(C).GetNestedType(""ConstrainedToStruct`1"", BindingFlags.Public).MakeGenericType(typeof(T))
                : typeof(C).GetNestedType(""ConstrainedToClass`1"", BindingFlags.Public).MakeGenericType(typeof(T));
        }

        public class ConstrainedToClass<T>
            where T : class
        {
        }

        public class ConstrainedToStruct<T>
            where T : struct
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void TernaryTwoArguments()
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public Type Get<T>()
        {
            return typeof(T).IsValueType
                ? typeof(C).GetNestedType(""ConstrainedToStruct`2"", BindingFlags.Public).MakeGenericType(typeof(T), typeof(C))
                : typeof(C).GetNestedType(""ConstrainedToClass`2"", BindingFlags.Public).MakeGenericType(typeof(T), typeof(C));
        }

        public class ConstrainedToClass<T1, T2>
            where T1 : class
        {
        }

        public class ConstrainedToStruct<T1, T2>
            where T1 : struct
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void IfElse()
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public Type Get<T>()
        {
            if (typeof(T).IsValueType)
            {
                return typeof(C).GetNestedType(""ConstrainedToStruct`1"", BindingFlags.Public).MakeGenericType(typeof(T));
            }
            else
            {
                return typeof(C).GetNestedType(""ConstrainedToClass`1"", BindingFlags.Public).MakeGenericType(typeof(T));
            }
        }

        public class ConstrainedToClass<T>
            where T : class
        {
        }

        public class ConstrainedToStruct<T>
            where T : struct
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void IfReturn()
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public Type Get<T>()
        {
            if (typeof(T).IsValueType)
            {
                return typeof(C).GetNestedType(""ConstrainedToStruct`1"", BindingFlags.Public).MakeGenericType(typeof(T));
            }

            return typeof(C).GetNestedType(""ConstrainedToClass`1"", BindingFlags.Public).MakeGenericType(typeof(T));
        }

        public class ConstrainedToClass<T>
            where T : class
        {
        }

        public class ConstrainedToStruct<T>
            where T : struct
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void NestedIf()
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public Type Get<T>()
        {
            if (typeof(T).IsValueType)
            {
                if (true)
                {
                    return typeof(C).GetNestedType(""ConstrainedToStruct`1"", BindingFlags.Public).MakeGenericType(typeof(T));
                }
            }

            return null;
        }

        public class ConstrainedToClass<T>
            where T : class
        {
        }

        public class ConstrainedToStruct<T>
            where T : struct
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
