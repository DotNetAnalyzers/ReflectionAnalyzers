namespace ReflectionAnalyzers.Tests.REFL040PreferIsInstanceOfTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IsAssignableFromAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL040PreferIsInstanceOfType.Descriptor;

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
