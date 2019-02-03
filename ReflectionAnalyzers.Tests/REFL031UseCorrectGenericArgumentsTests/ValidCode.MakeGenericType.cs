namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL031UseCorrectGenericArguments.Descriptor;

            [TestCase("string")]
            [TestCase("int")]
            [TestCase("int?")]
            [TestCase("Console")]
            public void SingleUnconstrained(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T>
    {
        public static void Bar()
        {
            var type = typeof(C<>).MakeGenericType(typeof(int));
        }
    }
}".AssertReplace("int", type);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : class", "typeof(string)")]
            [TestCase("where T : class", "typeof(Console)")]
            [TestCase("where T : struct", "typeof(int)")]
            [TestCase("where T : unmanaged", "typeof(int)")]
            [TestCase("where T : IComparable", "typeof(int)")]
            [TestCase("where T : IComparable<T>", "typeof(int)")]
            [TestCase("where T : new()", "typeof(C<int>)")]
            public void ConstrainedParameter(string constraint, string arg)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T>
        where T : class
    {
        public static object Get => typeof(C<>).MakeGenericType(typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T1 : class", "where T2 : T1", "typeof(object), typeof(int)")]
            public void TransitiveConstraints(string where1, string where2, string types)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T1, T2> 
        where T1 : class
        where T2 : T1
    {
        public static object Get => typeof(C<,>).MakeGenericType(typeof(object), typeof(int));
    }
}".AssertReplace("where T1 : class", where1)
  .AssertReplace("where T2 : T1", where2)
  .AssertReplace("typeof(object), typeof(int)", types);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void ImplicitDefaultConstructor()
            {
                var code = @"
namespace RoslynSandbox
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

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : Enum", "AttributeTargets")]
            [TestCase("where T : struct, System.Enum", "AttributeTargets")]
            [TestCase("where T : Enum", "Enum")]
            [TestCase("where T : unmanaged", "int")]
            [TestCase("where T : unmanaged", "Safe")]
            [TestCase("where T : unmanaged", "AttributeTargets")]
            public void ConstrainedToEnum(string constraint, string arg)
            {
                var safeCode = @"
namespace RoslynSandbox
{
    using System;

    public struct Safe
    {
        public int Value1;
        public AttributeTargets Value2;
    }
}";

                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T> 
        where T : Enum
    {
        public static object Get => typeof(C<>).MakeGenericType(typeof(AttributeTargets));
    }
}".AssertReplace("where T : Enum", constraint)
  .AssertReplace("AttributeTargets", arg);

                AnalyzerAssert.Valid(Analyzer, Descriptor, safeCode, code);
            }

            [Test]
            public void Recursion()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T1, T2>
        where T1 : T2
        where T2 : T1
    {
        public static void Bar()
        {
            var type = typeof(C<,>).MakeGenericType(typeof(int), typeof(int));
        }
    }
}";
                var solution = CodeFactory.CreateSolution(code, CodeFactory.DefaultCompilationOptions(Analyzer), AnalyzerAssert.MetadataReferences);
                AnalyzerAssert.NoDiagnostics(Analyze.GetDiagnostics(Analyzer, solution));
            }

            [Test]
            public void NestedType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar()
        {
            var type = typeof(C).GetNestedType(""Baz`1"").MakeGenericType(typeof(int));
        }

        public class Baz<T>
        {
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void NestedGenericInGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C<T>
    {
        public object Get => typeof(C<>.D<>).MakeGenericType(typeof(int), typeof(int));

        public class D<U>
        {
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void PassingArrayOfUnknownToMakeGenericType()
            {
                var code = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void Ternary()
            {
                var code = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, code);
            }

            [Test]
            public void TernaryTwoArguments()
            {
                var code = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, code);
            }

            [Test]
            public void IfElse()
            {
                var code = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, code);
            }

            [Test]
            public void IfReturn()
            {
                var code = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, code);
            }
        }
    }
}
