namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public void SingleUnconstrained()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C<T>
    {
        public static object Get() => typeof(C<>).MakeGenericType(typeof(int), typeof(double));
    }
}";
                var message = "The number of generic arguments provided doesn't equal the arity of the generic type definition. The member has 1 parameter but 2 arguments are passed in.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public void GetGenericTypeDefinition()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T>
    {
        public static object Get() => typeof(C<int>).GetGenericTypeDefinition().MakeGenericType↓(typeof(int), typeof(double));
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("where T : class", "int")]
            [TestCase("where T : class", "int?")]
            [TestCase("where T : struct", "string")]
            [TestCase("where T : struct", "int?")]
            [TestCase("where T : struct", "System.ValueType")]
            [TestCase("where T : struct", "System.Enum")]
            [TestCase("where T : struct", "System.Console")]
            [TestCase("where T : struct, System.Enum", "System.Enum")]
            [TestCase("where T : unmanaged", "object")]
            [TestCase("where T : unmanaged", "Console")]
            [TestCase("where T : unmanaged", "int?")]
            [TestCase("where T : IComparable", "C<int>")]
            [TestCase("where T : IComparable<double>", "C<int>")]
            [TestCase("where T : IComparable<double>", "int")]
            [TestCase("where T : new()", "Bar")]
            public void ConstrainedParameter(string constraint, string arg)
            {
                var bar = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public Bar(int i)
        {
        }
    }
}";

                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C<T>
        where T : class
    {
        public static object Get() => typeof(C<>).MakeGenericType(↓typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", $"typeof({arg})");

                var message = $"The argument typeof({arg}), on 'RoslynSandbox.C<>' violates the constraint of type 'T'.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), bar, code);
            }

            [TestCase("where T1 : class", "where T2 : T1", "typeof(IEnumerable), typeof(object)")]
            public void TransitiveConstraints(string where1, string where2, string types)
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Collections;

    public class C<T1, T2> 
        where T1 : class
        where T2 : T1
    {
        public static object Get => typeof(C<,>).MakeGenericType(typeof(IEnumerable), typeof(object));
    }
}".AssertReplace("where T1 : class", where1)
  .AssertReplace("where T2 : T1", where2)
  .AssertReplace("typeof(IEnumerable), typeof(object)", types);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
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
        public static object Get() => typeof(C).GetNestedType(""Baz`1"").MakeGenericType↓(typeof(int), typeof(double));

        public class Baz<T>
        {
        }
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void NestedGenericInGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C<T>
    {
        public object Get => typeof(C<>.D<>).MakeGenericType(typeof(int), typeof(int), typeof(int));

        public class D<U>
        {
        }
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void TernaryWrongOrder()
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
                ? typeof(C).GetNestedType(""ConstrainedToClass`1"", BindingFlags.Public).MakeGenericType(typeof(T))
                : typeof(C).GetNestedType(""ConstrainedToStruct`1"", BindingFlags.Public).MakeGenericType(typeof(T));
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
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void TernaryTwoArgumentsWrongOrder()
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
                ? typeof(C).GetNestedType(""ConstrainedToClass`2"", BindingFlags.Public).MakeGenericType(typeof(T), typeof(C))
                : typeof(C).GetNestedType(""ConstrainedToStruct`2"", BindingFlags.Public).MakeGenericType(typeof(T), typeof(C));
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
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
