namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MakeGenericType
        {
            private static readonly MakeGenericAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL034DoNotMakeGeneric);

            [Test]
            public static void WhenNotGeneric()
            {
                var code = @"
namespace N
{
    class C
    {
        public object Get => typeof(C).↓MakeGenericType(typeof(int));
    }
}";
                var message = "N.C is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void WhenNotGenericDefinition()
            {
                var code = @"
namespace N
{
    class C<T>
    {
        public object Get => typeof(C<int>).↓MakeGenericType(typeof(int));
    }
}";
                var message = "N.C<int> is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void Nested()
            {
                var code = @"
namespace N
{
    class C
    {
        public object Get => typeof(C.D).↓MakeGenericType(typeof(int));

        class D
        {
        }
    }
}";
                var message = "N.C.D is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void NestedInGeneric()
            {
                var code = @"
namespace N
{
    class C<T>
    {
        public object Get => typeof(C<int>.D).↓MakeGenericType(typeof(int));

        class D
        {
        }
    }
}";
                var message = "N.C<int>.D is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
