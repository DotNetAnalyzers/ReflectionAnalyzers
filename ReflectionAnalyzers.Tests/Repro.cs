namespace ReflectionAnalyzers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

#pragma warning disable GURA06 // Move test to correct class.
    [Explicit("Only for digging out test cases.")]
    public static class Repro
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(KnownSymbol).Assembly.GetTypes()
                               .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                               .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                               .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            new FileInfo(@"C:\Git\_GuOrg\Gu.Xml\Gu.Xml.sln"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void Run(DiagnosticAnalyzer analyzer)
        {
            var diagnostics = Analyze.GetDiagnostics(Solution, analyzer);
            RoslynAssert.NoDiagnostics(diagnostics);
        }
    }
}
