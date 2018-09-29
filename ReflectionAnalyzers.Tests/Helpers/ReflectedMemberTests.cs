namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ReflectedMemberTests
    {
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString))",                                                                             "Foo",           "typeof(Foo)")]
        [TestCase("new Foo().GetType().GetMethod(nameof(this.ToString))",                                                                     "Foo",           "new Foo().GetType()")]
        [TestCase("foo.GetType().GetMethod(nameof(this.ToString))",                                                                           "Foo",           "foo.GetType()")]
        [TestCase("this.GetType().GetMethod(nameof(this.ToString))",                                                                          "Foo",           "this.GetType()")]
        [TestCase("GetType().GetMethod(nameof(this.ToString))",                                                                               "Foo",           "GetType()")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\").GetMethod(nameof(this.ToString))",                                       "Int32",         "typeof(string).Assembly.GetType(\"System.Int32\")")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\").GetMethod(nameof(this.ToString))", "IEnumerable`1", "typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        public void TryGetTypeFromExpression(string call, string expected, string expectedSource)
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
            Assert.AreEqual(true, ReflectedMember.TryGetType(node, context, out var type, out var source));
            Assert.AreEqual(expected, type.MetadataName);
            Assert.AreEqual(expectedSource, source.Value.ToString());
        }

        [TestCase("typeof(Foo)", "Foo")]
        [TestCase("new Foo().GetType()", "Foo")]
        [TestCase("foo.GetType()", "Foo")]
        [TestCase("this.GetType()", "Foo")]
        [TestCase("GetType()", "Foo")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")", "Int32")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")", "IEnumerable`1")]
        public void TryGetTypeFromLocal(string typeExpression, string expected)
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
            Assert.AreEqual(true, ReflectedMember.TryGetType(node, context, out var type, out var instance));
            Assert.AreEqual(expected, type.MetadataName);
            Assert.AreEqual(typeExpression, instance.Value.ToString());
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
            Assert.AreEqual(false, ReflectedMember.TryGetType(node, context, out _, out _));
        }

        [Test]
        public void Dump()
        {
            Console.WriteLine(typeof(string).Assembly.GetType("System.Int32"));
        }
    }
}
