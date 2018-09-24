namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class GetXTests
    {
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString))", "Foo", null)]
        [TestCase("new Foo().GetType().GetMethod(nameof(this.ToString))", "Foo", null)]
        [TestCase("foo.GetType().GetMethod(nameof(this.ToString))", "Foo", "foo")]
        [TestCase("this.GetType().GetMethod(nameof(this.ToString))", "Foo", null)]
        [TestCase("GetType().GetMethod(nameof(this.ToString))", "Foo", null)]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\").GetMethod(nameof(this.ToString))", "Int32", null)]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\").GetMethod(nameof(this.ToString))", "IEnumerable`1", null)]
        public void TryGetTargetTypeExpression(string call, string expected, string expectedInstance)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.Reflection;

    class Foo
    {
        public Foo(Foo foo)
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(this.ToString))", call);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindInvocation(call);
            var context = new SyntaxNodeAnalysisContext(null, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, GetX.TryGetType(node, context, out var type, out var instance));
            Assert.AreEqual(expected, type.MetadataName);
            if (expectedInstance == null)
            {
                Assert.AreEqual(false, instance.HasValue);
            }
            else
            {
                Assert.AreEqual(expectedInstance, instance.Value.ToString());
            }
        }

        [TestCase("typeof(Foo)", "Foo", null)]
        [TestCase("new Foo().GetType()", "Foo", null)]
        [TestCase("foo.GetType()", "Foo", "foo")]
        [TestCase("this.GetType()", "Foo", null)]
        [TestCase("GetType()", "Foo", null)]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")", "Int32", null)]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")", "IEnumerable`1", null)]
        public void TryGetTargetTypeLocal(string typeExpression, string expected, string expectedInstance)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.Reflection;

    class Foo
    {
        public Foo(Foo foo)
        {
            var type = typeof(Foo);
            var methodInfo = type.GetMethod(nameof(this.ToString));
        }
    }
}".AssertReplace("typeof(Foo)", typeExpression);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(null, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, GetX.TryGetType(node, context, out var type, out var instance));
            Assert.AreEqual(expected, type.MetadataName);
            if (expectedInstance == null)
            {
                Assert.AreEqual(false, instance.HasValue);
            }
            else
            {
                Assert.AreEqual(expectedInstance, instance.Value.ToString());
            }
        }

        [Test]
        public void Recursion()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo()
        {
            Type type;
            type = type;
            var methodInfo = type.GetMethod(nameof(this.ToString));
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(null, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(false, GetX.TryGetType(node, context, out _, out _));
        }

        [Test]
        public void Dump()
        {
            Console.WriteLine(typeof(string).Assembly.GetType("System.Int32"));
        }
    }
}
