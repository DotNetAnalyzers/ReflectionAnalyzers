namespace ReflectionAnalyzers.Tests.Helpers.Filters
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class TypesTests
    {
        [TestCase("new[] { typeof(int) }", "M(IFormattable _)", "M(object _)")]
        [TestCase("new[] { typeof(object) }", "M(object _)", "M(IFormattable _)")]
        [TestCase("new[] { typeof(int) }", "M(int _)", "M(object _)")]
        [TestCase("new[] { typeof(int) }", "M(int _)", "M(IFormattable _)")]
        public static void TryMostSpecific(string filterType, string signature1, string signature2)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public object Get() => typeof(C).GetMethod(nameof(this.M), new[] { typeof(FilterType) });

        public void M(Type1 _) { }

        public void M(Type2 _) { }
    }
}".AssertReplace("new[] { typeof(FilterType) }", filterType)
  .AssertReplace("M(Type1 _)", signature1)
  .AssertReplace("M(Type2 _)", signature2);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var m1 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature1), CancellationToken.None);
            var m2 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature2), CancellationToken.None);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            Assert.AreEqual(true, Types.TryCreate(invocation, (IMethodSymbol?)semanticModel.GetSymbolInfo(invocation).Symbol, semanticModel, CancellationToken.None, out var types));
            Assert.AreEqual(true, types.TryMostSpecific(m1, m2, out var match));
            Assert.AreEqual(m1, match);
            Assert.AreEqual(true, types.TryMostSpecific(m2, m1, out match));
            Assert.AreEqual(m1, match);
        }

        [TestCase("new[] { typeof(int), typeof(int) }", "M(IFormattable _, object __)", "M(object _, IFormattable __)")]
        public static void TryMostSpecificWhenAmbiguous(string filterTypes, string signature1, string signature2)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public object Get() => typeof(C).GetMethod(nameof(this.M), new[] { typeof(FilterType) });

        public void M(Type1 _) { }

        public void M(Type2 _) { }
    }
}".AssertReplace("new[] { typeof(FilterType) }", filterTypes)
  .AssertReplace("M(Type1 _)", signature1)
  .AssertReplace("M(Type2 _)", signature2);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var m1 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature1), CancellationToken.None);
            var m2 = semanticModel.GetDeclaredSymbol(syntaxTree.FindMethodDeclaration(signature2), CancellationToken.None);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            Assert.AreEqual(true, Types.TryCreate(invocation, (IMethodSymbol?)semanticModel.GetSymbolInfo(invocation).Symbol, semanticModel, CancellationToken.None, out var types));
            Assert.AreEqual(false, types.TryMostSpecific(m1, m2, out _));
        }
    }
}
