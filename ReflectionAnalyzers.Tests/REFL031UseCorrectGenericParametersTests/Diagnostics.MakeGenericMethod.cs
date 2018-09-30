namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MakeGenericMethod
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public void SingleUnconstrainedTwoTypeArguments()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
        {
            var method = typeof(Foo).GetMethod(nameof(Foo.Bar)).MakeGenericMethod↓(typeof(int), typeof(double));
        }
    }
}";
                var message = "Use correct generic parameters.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("MakeGenericMethod↓(new[] { typeof(int), typeof(double) })")]
            [TestCase("MakeGenericMethod(↓typeof(string))")]
            [TestCase("MakeGenericMethod(new[] { ↓typeof(string) })")]
            public void ConstrainedParameterWrongArguments(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
            where T : struct
        {
            var method = typeof(Foo).GetMethod(nameof(Foo.Bar)).MakeGenericMethod(↓typeof(string));
        }
    }
}".AssertReplace("MakeGenericMethod(↓typeof(string))", call);
                var message = "Use correct generic parameters.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("where T : class", "typeof(int)")]
            [TestCase("where T : struct", "typeof(string)")]
            [TestCase("where T : IComparable", "typeof(Foo)")]
            [TestCase("where T : new()", "typeof(Bar)")]
            public void Constraints(string constraint, string arg)
            {
                var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public Bar(int i)
        {
        }
    }
}";
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
            where T : class
        {
            var method = typeof(Foo).GetMethod(nameof(Foo.Bar)).MakeGenericMethod(↓typeof(int));
        }
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);
                var message = "Use correct generic parameters.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), barCode, code);
            }
        }
    }
}
