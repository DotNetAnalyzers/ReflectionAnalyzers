namespace ReflectionAnalyzers.Tests.REFL040PreferIsInstanceOfTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IsAssignableFromAnalyzer();
        private static readonly CodeFixProvider Fix = new UseIsInstanceOfTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL040PreferIsInstanceOfType.Descriptor);

        [Test]
        public static void IsAssignableFromInstanceGetType()
        {
            var before = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public bool M(Type t1, object o) => ↓t1.IsAssignableFrom(o.GetType());
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public bool M(Type t1, object o) => t1.IsInstanceOfType(o);
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
