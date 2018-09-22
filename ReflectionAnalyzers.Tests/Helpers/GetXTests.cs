namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class GetXTests
    {
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString))",                                                                             "Foo")]
        [TestCase("new Foo().GetType().GetMethod(nameof(this.ToString))",                                                                     "Foo")]
        [TestCase("this.GetType().GetMethod(nameof(this.ToString))",                                                                          "Foo")]
        [TestCase("GetType().GetMethod(nameof(this.ToString))",                                                                               "Foo")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\").GetMethod(nameof(this.ToString))",                                       "Int32")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\").GetMethod(nameof(this.ToString))", "IEnumerable`1")]
        public void TryGetTargetTypeExpression(string call, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(this.ToString))", call);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindInvocation(call);
            Assert.AreEqual(true,     GetX.TryGetTargetType(node, semanticModel, CancellationToken.None, out var type));
            Assert.AreEqual(expected, type.MetadataName);
        }

        [TestCase("typeof(Foo)",                                                                             "Foo")]
        [TestCase("new Foo().GetType()",                                                                     "Foo")]
        [TestCase("foo.GetType()",                                                                           "Foo")]
        [TestCase("this.GetType()",                                                                          "Foo")]
        [TestCase("GetType()",                                                                               "Foo")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")",                                       "Int32")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")", "IEnumerable`1")]
        public void TryGetTargetTypeLocal(string typeExpression, string expected)
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
            Assert.AreEqual(true,     GetX.TryGetTargetType(node, semanticModel, CancellationToken.None, out var type));
            Assert.AreEqual(expected, type.MetadataName);
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
            Assert.AreEqual(false, GetX.TryGetTargetType(node, semanticModel, CancellationToken.None, out _));
        }

        [Test]
        public void Dump()
        {
            Console.WriteLine(typeof(string).Assembly.GetType("System.Int32"));
        }
    }
}
