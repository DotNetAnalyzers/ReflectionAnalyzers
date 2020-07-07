#pragma warning disable GURA04,GURA06
namespace ReflectionAnalyzers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCodeWithAllAnalyzers
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(KnownSymbol)
                                                                                 .Assembly.GetTypes()
                                                                                 .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                 .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t)!)
                                                                                 .ToArray();

        private static readonly Solution AnalyzersProjectSolution = CodeFactory.CreateSolution(
            ProjectFile.Find("ReflectionAnalyzers.csproj"),
            AllAnalyzers,
            metadataReferences: MetadataReferences.FromAttributes());

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [Test]
        public static void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void ValidCodeProject(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, ValidCodeProjectSln);
        }

        [Ignore("Not working with nullable attributes.")]
        [TestCaseSource(nameof(AllAnalyzers))]
        public static void AnalyzersSolution(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, AnalyzersProjectSolution);
        }
    }
}
