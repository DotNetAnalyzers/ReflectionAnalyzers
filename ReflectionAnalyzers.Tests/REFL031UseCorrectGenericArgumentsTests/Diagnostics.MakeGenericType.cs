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
    public class Foo<T>
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(typeof(int), typeof(double));
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

    public class Foo<T>
    {
        public static object Get() => typeof(Foo<int>).GetGenericTypeDefinition().MakeGenericType↓(typeof(int), typeof(double));
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
            [TestCase("where T : struct, System.Enum", "System.Enum")]
            [TestCase("where T : unmanaged", "object")]
            [TestCase("where T : unmanaged", "NotSafe")]
            [TestCase("where T : IComparable", "Foo<int>")]
            [TestCase("where T : IComparable<double>", "Foo<int>")]
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

                var notSafe = @"
namespace RoslynSandbox
{
    public struct NotSafe
    {
        public object value;
    }
}";

                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
        where T : class
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(↓typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", $"typeof({arg})");

                var message = $"The argument typeof({arg}), on 'RoslynSandbox.Foo<>' violates the constraint of type 'T'.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), bar, notSafe, code);
            }

            [Test]
            public void RefStruct()
            {
                var refStruct = @"
namespace RoslynSandbox
{
    public ref struct RefStruct
    {
        public int Value;
    }
}";
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(↓typeof(RefStruct));
    }
}";
                var message = "The argument typeof(RefStruct), on 'RoslynSandbox.Foo<>' violates the constraint of type 'T'.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), refStruct, code);
            }

            [Test]
            public void Static()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(↓typeof(Console));
    }
}";
                var message = "The argument typeof(Console), on 'RoslynSandbox.Foo<>' violates the constraint of type 'T'.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public void TransitiveConstraints()
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
}";

                var message = $"The argument typeof(), on 'RoslynSandbox.Foo<>' violates the constraint of type 'T'.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
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
        public static object Get() => typeof(Foo).GetNestedType(""Baz`1"").MakeGenericType↓(typeof(int), typeof(double));

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
        }
    }
}
