namespace ReflectionAnalyzers.Tests.Helpers.Reflection;

using System.Threading;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class MethodInfoTests
{
    [TestCase("typeof(C).GetMethod(nameof(this.M))",                              "N.C.M()")]
    [TestCase("c.GetType().GetMethod(nameof(this.M))",                          "N.C.M()")]
    [TestCase("cType.GetMethod(nameof(this.M))",                                "N.C.M()")]
    [TestCase("Cached",                                                           "N.C.M()")]
    [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetMethod",           "N.C.Property.get")]
    [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetGetMethod(false)", "N.C.Property.get")]
    [TestCase("typeof(C).GetProperty(nameof(this.Property)).SetMethod",           "N.C.Property.set")]
    [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetSetMethod(false)", "N.C.Property.set")]
    public static void TryGet(string call, string expected)
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class C
    {
        private static readonly MethodInfo Cached = typeof(C).GetMethod(nameof(M));

        public C(C c)
        {
            var cType = typeof(C);
            var mi = typeof(C).GetMethod(nameof(this.M));
        }

        public int M() => 1;

        public int Property { get; set; }
    }
}".AssertReplace("typeof(C).GetMethod(nameof(this.M))", call);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var node = syntaxTree.FindExpression(call);
        Assert.AreEqual(expected, MethodInfo.Find(node, semanticModel, CancellationToken.None)?.Method.ToString());
    }
}
