namespace ReflectionAnalyzers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Gu.Roslyn.Asserts;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public static class AllAnalyzersValid
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(KnownSymbol)
                .Assembly
                .GetTypes()
                .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t)!)
                .ToArray();

        private static readonly Solution AnalyzersProject = CodeFactory.CreateSolution(
            ProjectFile.Find("ReflectionAnalyzers.csproj"),
            settings: LibrarySettings.Roslyn);

        private static readonly Solution ValidCodeProject = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"));

        [Test]
        public static void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void ForAnalyzersProject(DiagnosticAnalyzer analyzer)
        {
            Assert.Inconclusive("Does not figure out source package.");
            RoslynAssert.Valid(analyzer, AnalyzersProject);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void ForValidCode(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, ValidCodeProject);
        }
    }
}
