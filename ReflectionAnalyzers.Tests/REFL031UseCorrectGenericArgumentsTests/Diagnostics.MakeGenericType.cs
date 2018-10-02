namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public void SingleUnconstrained()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo<T>
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(typeof(int), typeof(double));
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void GetGenericTypeDefinition()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
    {
        public static object Get() => typeof(Foo<int>).GetGenericTypeDefinition().MakeGenericType↓(typeof(int), typeof(double));
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("where T : class",       "typeof(int)")]
            [TestCase("where T : struct",      "typeof(string)")]
            [TestCase("where T : IComparable", "typeof(Foo<int>)")]
            [TestCase("where T : new()",       "typeof(Bar)")]
            public void ConstrainedParameter(string constraint, string arg)
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

    public class Foo<T>
        where T : class
    {
        public static object Get() => typeof(Foo<>).MakeGenericType(↓typeof(int));
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, code);
            }

            [Test]
            public void NestedType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static object Get() => typeof(Foo).GetNestedType(""Baz`1"").MakeGenericType↓(typeof(int), typeof(double));

        public class Baz<T>
        {
        }
    }
}";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
