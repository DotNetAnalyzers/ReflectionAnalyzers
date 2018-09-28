namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericTypeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public void SingleUnconstrained()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
    {
        public static void Bar()
        {
            var type = typeof(Foo<>).MakeGenericType(typeof(int));
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("where T : class",       "typeof(string)")]
            [TestCase("where T : struct",      "typeof(int)")]
            [TestCase("where T : IComparable", "typeof(int)")]
            [TestCase("where T : new()",       "typeof(Foo<int>)")]
            public void ConstrainedParameter(string constraint, string arg)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T>
        where T : class
    {
        public static void Bar()
        {
            var type = typeof(Foo<>).MakeGenericType(typeof(int));
        }
    }
}".AssertReplace("where T : class", constraint).AssertReplace("typeof(int)", arg);

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void Recursion()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo<T1, T2>
        where T1 : T2
        where T2 : T1
    {
        public static void Bar()
        {
            var type = typeof(Foo<,>).MakeGenericType(typeof(int), typeof(int));
        }
    }
}";
                var solution = CodeFactory.CreateSolution(code, CodeFactory.DefaultCompilationOptions(Analyzer), AnalyzerAssert.MetadataReferences);
                AnalyzerAssert.NoDiagnostics(Analyze.GetDiagnostics(Analyzer, solution));
            }
        }
    }
}
