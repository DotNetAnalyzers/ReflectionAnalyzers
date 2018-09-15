namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL005");

        [TestCase("GetMethod(nameof(this.ToString))")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Public)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Static |BindingFlags.Public)")]
        public void GetToString(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }
    }
}".AssertReplace("GetMethod(nameof(this.ToString))", call);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void GetPrivate()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void Bar()
        {
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void GetBarOverloaded()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar));
        }

        public void Bar()
        {
        }

        public int Bar(int i) => i;
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void UnknownType()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo(Type type)
        {
            var methodInfo = type.GetMethod(""Bar"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
