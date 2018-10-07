namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
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
    using System;

    public class Foo<T>
    {
        public static void Bar()
        {
            var type = typeof(Foo<>).MakeGenericType(typeof(int));
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("where T : class",          "typeof(string)")]
            [TestCase("where T : struct",         "typeof(int)")]
            [TestCase("where T : IComparable",    "typeof(int)")]
            [TestCase("where T : IComparable<T>", "typeof(int)")]
            [TestCase("where T : new()",          "typeof(Foo<int>)")]
            public void ConstrainedParameter(string constraint, string arg)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
        where T : class
    {
        public static object Get => typeof(Foo<>).MakeGenericType(typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void TransitiveConstraints()
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
}";

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
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
        public static object Get => typeof(C<>).MakeGenericType(typeof(HasImplicitDefaultConstructor));
    }
}";

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void Recursion()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T1, T2>
        where T1 : T2
        where T2 : T1
    {
        public static void Bar()
        {
            var type = typeof(Foo<,>).MakeGenericType(typeof(int), typeof(int));
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

    public class Foo
    {
        public static void Bar()
        {
            var type = typeof(Foo).GetNestedType(""Baz`1"").MakeGenericType(typeof(int));
        }

        public class Baz<T>
        {
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
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
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
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
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
