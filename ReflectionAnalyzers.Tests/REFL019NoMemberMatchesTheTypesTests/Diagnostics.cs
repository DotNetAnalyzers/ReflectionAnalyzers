namespace ReflectionAnalyzers.Tests.REFL019NoMemberMatchesTheTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL019NoMemberMatchesTheTypes.Descriptor);

        [TestCase("GetConstructor(↓Type.EmptyTypes)")]
        [TestCase("GetConstructor(↓Array.Empty<Type>())")]
        [TestCase("GetConstructor(↓new Type[0])")]
        [TestCase("GetConstructor(↓new Type[1] { typeof(double) })")]
        [TestCase("GetConstructor(↓new Type[] { typeof(double) })")]
        [TestCase("GetConstructor(↓new[] { typeof(double) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, ↓Type.EmptyTypes, null)")]
        public void GetConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

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
