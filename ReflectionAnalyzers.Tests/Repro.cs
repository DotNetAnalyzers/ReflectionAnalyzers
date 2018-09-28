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

    [Explicit("Only for digging out test cases.")]
    internal class Repro
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(KnownSymbol).Assembly.GetTypes()
                               .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                               .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                               .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            new FileInfo("C:\\Git\\Gu.Xml\\Gu.Xml.sln"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        [TestCaseSource(nameof(AllAnalyzers))]
        public void Run(DiagnosticAnalyzer analyzer)
        {
            var diagnostics = Analyze.GetDiagnostics(Solution, analyzer);
            AnalyzerAssert.NoDiagnostics(diagnostics);
        }
    }
}
