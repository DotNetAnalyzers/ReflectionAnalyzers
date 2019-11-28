namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        public static class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL034DoNotMakeGeneric;

            [Test]
            public static void Vanilla()
            {
                var code = @"
namespace N
{
    class C<T>
    {
        public object Get => typeof(C<>).MakeGenericType(typeof(int));
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void Constrained()
            {
                var code = @"
namespace N
{
    using System;

    class C<T>
        where T : IComparable<T>
    {
        public object Get => typeof(C<>).MakeGenericType(typeof(int));
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void Nested()
            {
                var code = @"
namespace N
{
    using System;

    class C
    {
        public object Get => typeof(C.D<>).MakeGenericType(typeof(int));

        class D<T>
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void NestedInGeneric()
            {
                var code = @"
namespace N
{
    using System;

    class C<T>
    {
        public object Get => typeof(C<>.D).MakeGenericType(typeof(int));

        class D
        {
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
