namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL005WrongBindingFlags.Descriptor);

        [TestCase("GetConstructor(↓Type.EmptyTypes)")]
        [TestCase("GetConstructor(↓Array.Empty<Type>())")]
        [TestCase("GetConstructor(↓new Type[0])")]
        [TestCase("GetConstructor(↓new Type[1] { typeof(double) })")]
        [TestCase("GetConstructor(↓new Type[] { typeof(double) })")]
        [TestCase("GetConstructor(↓new[] { typeof(double) })")]
        public void GetConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(int value)
        {
            var ctor = typeof(Foo).GetConstructor(↓Type.EmptyTypes);
        }
    }
}".AssertReplace("GetConstructor(↓Type.EmptyTypes)", call);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
