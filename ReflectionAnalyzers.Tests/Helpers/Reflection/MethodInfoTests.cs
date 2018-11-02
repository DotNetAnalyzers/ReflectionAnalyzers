namespace ReflectionAnalyzers.Tests.Helpers.Reflection
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class MethodInfoTests
    {
        [TestCase("typeof(C).GetMethod(nameof(this.M))",                              "RoslynSandbox.C.M()")]
        [TestCase("foo.GetType().GetMethod(nameof(this.M))",                            "RoslynSandbox.C.M()")]
        [TestCase("fooType.GetMethod(nameof(this.M))",                                  "RoslynSandbox.C.M()")]
        [TestCase("Cached",                                                             "RoslynSandbox.C.M()")]
        [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetMethod",           "RoslynSandbox.C.Property.get")]
        [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetGetMethod(false)", "RoslynSandbox.C.Property.get")]
        [TestCase("typeof(C).GetProperty(nameof(this.Property)).SetMethod",           "RoslynSandbox.C.Property.set")]
        [TestCase("typeof(C).GetProperty(nameof(this.Property)).GetSetMethod(false)", "RoslynSandbox.C.Property.set")]
        public void TryGet(string call, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class C
    {
        private static readonly MethodInfo Cached = typeof(C).GetMethod(nameof(M));

        public C(C foo)
        {
            var fooType = typeof(C);
            var mi = typeof(C).GetMethod(nameof(this.M));
        }

        public int M() => 1;

        public int Property { get; set; }
    }
}".AssertReplace("typeof(C).GetMethod(nameof(this.M))", call);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindExpression(call);
            var context = new SyntaxNodeAnalysisContext(null, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true,     MethodInfo.TryGet(node, context, out var methodInfo));
            Assert.AreEqual(expected, methodInfo.Method.ToString());
        }
    }
}
