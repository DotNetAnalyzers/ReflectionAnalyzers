namespace ReflectionAnalyzers.Tests.Helpers.Reflection
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class MethodInfoTests
    {
        [TestCase("typeof(Foo).GetMethod(nameof(this.M))",                              "RoslynSandbox.Foo.M()")]
        [TestCase("foo.GetType().GetMethod(nameof(this.M))",                            "RoslynSandbox.Foo.M()")]
        [TestCase("fooType.GetMethod(nameof(this.M))",                                  "RoslynSandbox.Foo.M()")]
        [TestCase("Cached",                                                             "RoslynSandbox.Foo.M()")]
        [TestCase("typeof(Foo).GetProperty(nameof(this.Property)).GetMethod",           "RoslynSandbox.Foo.Property.get")]
        [TestCase("typeof(Foo).GetProperty(nameof(this.Property)).GetGetMethod(false)", "RoslynSandbox.Foo.Property.get")]
        [TestCase("typeof(Foo).GetProperty(nameof(this.Property)).SetMethod",           "RoslynSandbox.Foo.Property.set")]
        [TestCase("typeof(Foo).GetProperty(nameof(this.Property)).GetSetMethod(false)", "RoslynSandbox.Foo.Property.set")]
        public void TryGet(string call, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Foo
    {
        private static readonly MethodInfo Cached = typeof(Foo).GetMethod(nameof(M));

        public Foo(Foo foo)
        {
            var fooType = typeof(Foo);
            var mi = typeof(Foo).GetMethod(nameof(this.M));
        }

        public int M() => 1;

        public int Property { get; set; }
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(this.M))", call);
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
