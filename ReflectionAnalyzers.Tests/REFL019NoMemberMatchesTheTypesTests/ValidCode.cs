namespace ReflectionAnalyzers.Tests.REFL019NoMemberMatchesTheTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL019NoMemberMatchesTheTypes.Descriptor);

        [TestCase("GetConstructor(new[] { typeof(int) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
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
            var ctor = typeof(Foo).GetConstructor(new[] { typeof(int) });
        }
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) })", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetConstructor(Type.EmptyTypes)")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
        [TestCase("GetConstructor(new[] { typeof(int) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
        [TestCase("GetConstructor(new[] { typeof(double) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null)")]
        public void GetConstructorWhenOverloaded(string call)
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
            var ctor = typeof(Foo).GetConstructor(Type.EmptyTypes);
        }

        public Foo(int value)
        {
        }

        public Foo(double value)
        {
        }
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
