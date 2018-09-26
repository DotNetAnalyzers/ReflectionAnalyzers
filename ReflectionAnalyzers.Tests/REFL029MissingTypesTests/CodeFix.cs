namespace ReflectionAnalyzers.Tests.REFL029MissingTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new AddTypesFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL029MissingTypes.Descriptor);

        [Test]
        public void GetMethodNoParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar));
        }

        public int Bar() => 0;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), Type.EmptyTypes);
        }

        public int Bar() => 0;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void GetMethodOneParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Id));
        }

        public int Id(int value) => value;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Id), new[] { typeof(int) });
        }

        public int Id(int value) => value;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void GetMethodTwoParameters()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Add));
        }

        public double Add(int i, double d) => i + d;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Add), new[] { typeof(int), typeof(double) });
        }

        public double Add(int i, double d) => i + d;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
