namespace ReflectionAnalyzers.Tests.REFL027PreferTypeEmptyTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly CodeFixProvider Fix = new PreferEmptyTypesFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL027PreferTypeEmptyTypes.Descriptor);

        [TestCase("new Type[0]")]
        [TestCase("Array.Empty<Type>()")]
        [TestCase("new Type[0] { }")]
        [TestCase("new Type[] { }")]
        public void GetConstructor(string emptyArray)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetConstructor(new Type[0]);
        }
    }
}".AssertReplace("new Type[0]", emptyArray);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetConstructor(Type.EmptyTypes);
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
