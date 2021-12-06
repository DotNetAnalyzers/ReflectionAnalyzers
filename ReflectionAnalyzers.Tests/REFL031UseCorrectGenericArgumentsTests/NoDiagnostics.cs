namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class NoDiagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();

        [Test]
        public static void Recursion()
        {
            var code = @"
namespace N
{
    using System;

    public class C<T1, T2>
        where T1 : T2
        where T2 : T1
    {
        public static void M()
        {
            var type = typeof(C<,>).MakeGenericType(typeof(int), typeof(int));
        }
    }
}";
            var solution = CodeFactory.CreateSolution(code);
            RoslynAssert.NoAnalyzerDiagnostics(Analyzer, solution);
        }
    }
}
