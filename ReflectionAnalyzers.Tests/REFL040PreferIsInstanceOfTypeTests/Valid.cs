namespace ReflectionAnalyzers.Tests.REFL040PreferIsInstanceOfTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly IsAssignableFromAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL040PreferIsInstanceOfType;

        [Test]
        public static void UnknownTypesIsAssignableFrom()
        {
            var code = @"
namespace N
{
    using System;

    class C
    {
        public bool M(Type t1, Type t2) => t1.IsAssignableFrom(t2);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void UnknownTypesIsInstanceOfType()
        {
            var code = @"
namespace N
{
    using System;

    class C
    {
        public bool M(Type t1, C c) => t1.IsInstanceOfType(c);
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
