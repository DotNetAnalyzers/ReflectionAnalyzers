namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL004");

        [TestCase("GetProperty↓(\"Item\")")]
        [TestCase("GetProperty↓(\"Item\", BindingFlags.Public | BindingFlags.Instance)")]
        public void IndexerAndPropertyNamedItem(string call)
        {
            var baseCode = @"
namespace RoslynSandbox
{
    public class Base
    {
        public int Item { get; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : Base
    {
        public Foo()
        {
            _ = typeof(Foo).GetProperty↓(""Item"");
        }

        public int this[int i] => 0;
    }
}".AssertReplace("GetProperty↓(\"Item\")", call);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, baseCode, code);
        }
    }
}
