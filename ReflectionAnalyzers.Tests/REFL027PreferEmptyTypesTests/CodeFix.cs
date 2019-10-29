namespace ReflectionAnalyzers.Tests.REFL027PreferEmptyTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly CodeFixProvider Fix = new PreferEmptyTypesFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL027PreferEmptyTypes.Descriptor);

        [TestCase("new Type[0]")]
        [TestCase("Array.Empty<Type>()")]
        [TestCase("new Type[0] { }")]
        [TestCase("new Type[] { }")]
        public static void GetConstructor(string emptyArray)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            _ = typeof(C).GetConstructor(new Type[0]);
        }
    }
}".AssertReplace("new Type[0]", emptyArray);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            _ = typeof(C).GetConstructor(Type.EmptyTypes);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
