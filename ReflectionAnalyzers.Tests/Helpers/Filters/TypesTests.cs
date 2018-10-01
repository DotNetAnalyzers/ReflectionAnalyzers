namespace ReflectionAnalyzers.Tests.Helpers.Filters
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class TypesTests
    {
        [TestCase("object", "object", "IFormattable")]
        [TestCase("int", "int", "object")]
        [TestCase("int", "int", "IFormattable")]
        public void TryMostSpecific(string filterType, string type1, string type2)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(this.Bar), new[] { typeof(FilterType) });

        public void Bar(Type1 _) { }

        public void Bar(Type2 _) { }
    }
}".AssertReplace("FilterType", filterType)
  .AssertReplace("Type1", type1)
  .AssertReplace("Type2", type2);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var m1 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration("Bar(" + type1), CancellationToken.None);
            var m2 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration("Bar(" + type2), CancellationToken.None);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, Types.TryCreate(invocation, (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol, context, out var types));
            Assert.AreEqual(true, types.TryMostSpecific(m1, m2, out var match));
            Assert.AreEqual(m1, match);
            Assert.AreEqual(true, types.TryMostSpecific(m2, m1, out match));
            Assert.AreEqual(m1, match);
        }
    }
}
