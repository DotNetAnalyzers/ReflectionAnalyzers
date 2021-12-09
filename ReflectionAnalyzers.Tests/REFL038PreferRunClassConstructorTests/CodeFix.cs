namespace ReflectionAnalyzers.Tests.REFL038PreferRunClassConstructorTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly UseRunClassConstructorFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL038PreferRunClassConstructor);

        [Test]
        public static void WhenInvokingStatic()
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        static C()
        {
        }

        public static void M()
        {
            ↓typeof(C).GetConstructor(BindingFlags.NonPublic| BindingFlags.Static, null, Type.EmptyTypes, null).Invoke(null, null);
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        static C()
        {
        }

        public static void M()
        {
            RuntimeHelpers.RunClassConstructor(typeof(C).TypeHandle);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
