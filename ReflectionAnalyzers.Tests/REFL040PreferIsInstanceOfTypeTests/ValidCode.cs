namespace ReflectionAnalyzers.Tests.REFL040PreferIsInstanceOfTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IsAssignableFromAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL040PreferIsInstanceOfType.Descriptor;

        [Test]
        public void UnknownTypesIsAssignableFrom()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public bool M(Type t1, Type t2) => t1.IsAssignableFrom(t2);
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void UnknownTypesIsInstanceOfType()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public bool M(Type t1, C c) => t1.IsInstanceOfType(c);
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
