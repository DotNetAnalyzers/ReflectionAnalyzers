namespace ReflectionAnalyzers.Tests.REFL006RedundantBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL006");

        [Test]
        public void GetToString()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void GetReferenceEquals()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void Bar()
        {
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void UnknownTypeNoFlags()
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

        [Test]
        public void UnknownTypeStaticAndInstance()
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
            var methodInfo = type.GetMethod(""Bar"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
