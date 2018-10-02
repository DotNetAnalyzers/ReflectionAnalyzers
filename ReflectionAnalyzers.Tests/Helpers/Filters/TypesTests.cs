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
        [TestCase("new[] { typeof(int) }",    "Bar(IFormattable _)", "Bar(object _)")]
        [TestCase("new[] { typeof(object) }", "Bar(object _)",       "Bar(IFormattable _)")]
        [TestCase("new[] { typeof(int) }",    "Bar(int _)",          "Bar(object _)")]
        [TestCase("new[] { typeof(int) }",    "Bar(int _)",          "Bar(IFormattable _)")]
        public void TryMostSpecific(string filterType, string signature1, string signature2)
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
}".AssertReplace("new[] { typeof(FilterType) }", filterType)
  .AssertReplace("Bar(Type1 _)", signature1)
  .AssertReplace("Bar(Type2 _)", signature2);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var m1 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature1), CancellationToken.None);
            var m2 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature2), CancellationToken.None);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, Types.TryCreate(invocation, (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol, context, out var types));
            Assert.AreEqual(true, types.TryMostSpecific(m1, m2, out var match));
            Assert.AreEqual(m1,   match);
            Assert.AreEqual(true, types.TryMostSpecific(m2, m1, out match));
            Assert.AreEqual(m1,   match);
        }

        [TestCase("new[] { typeof(int), typeof(int) }", "Bar(IFormattable _, object __)", "Bar(object _, IFormattable __)")]
        public void TryMostSpecificWhenAmbiguous(string filterTypes, string signature1, string signature2)
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
}".AssertReplace("new[] { typeof(FilterType) }", filterTypes)
  .AssertReplace("Bar(Type1 _)", signature1)
  .AssertReplace("Bar(Type2 _)", signature2);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var m1 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature1), CancellationToken.None);
            var m2 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature2), CancellationToken.None);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, Types.TryCreate(invocation, (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol, context, out var types));
            Assert.AreEqual(false, types.TryMostSpecific(m1, m2, out _));
        }
    }
}
