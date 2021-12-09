namespace ReflectionAnalyzers.Tests.REFL040PreferIsInstanceOfTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly IsAssignableFromAnalyzer Analyzer = new();
        private static readonly UseIsInstanceOfTypeFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL040PreferIsInstanceOfType);

        [Test]
        public static void IsAssignableFromInstanceGetType()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public bool M(Type t1, object o) => ↓t1.IsAssignableFrom(o.GetType());
    }
}";

            var after = @"
namespace N
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
