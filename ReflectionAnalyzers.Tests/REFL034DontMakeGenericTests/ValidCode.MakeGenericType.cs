namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL034DontMakeGeneric.Descriptor;

            [Test]
            public void Vanilla()
            {
                var code = @"
namespace RoslynSandbox
{
    class C<T>
    {
        public object Get => typeof(C<>).MakeGenericType(typeof(int));
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void Constrained()
            {
                var code = @"
namespace RoslynSandbox
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
            public void Nested()
            {
                var code = @"
namespace RoslynSandbox
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
            public void NestedInGeneric()
            {
                var code = @"
namespace RoslynSandbox
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
