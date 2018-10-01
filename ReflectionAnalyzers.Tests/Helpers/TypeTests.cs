namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class TypeTests
    {
        [TestCase("typeof(Foo)",                                                                             "RoslynSandbox.Foo",                         "typeof(Foo)")]
        [TestCase("new Foo().GetType()",                                                                     "RoslynSandbox.Foo",                         "new Foo().GetType()")]
        [TestCase("foo.GetType()",                                                                           "RoslynSandbox.Foo",                         "foo.GetType()")]
        [TestCase("this.GetType()",                                                                          "RoslynSandbox.Foo",                         "this.GetType()")]
        [TestCase("GetType()",                                                                               "RoslynSandbox.Foo",                         "GetType()")]
        [TestCase("typeof(Foo).GetNestedType(\"Baz`1\")",                                                    "RoslynSandbox.Foo.Baz<T>",                  "typeof(Foo).GetNestedType(\"Baz`1\")")]
        [TestCase("typeof(Foo).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))",                       "RoslynSandbox.Foo.Baz<int>",                "typeof(Foo).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")",                                       "int",                                       "typeof(string).Assembly.GetType(\"System.Int32\")")]
        [TestCase("new Exception().GetType()",                                                               "Exception",                                 "new Exception().GetType()")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")", "System.Collections.Generic.IEnumerable<T>", "typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString)).ReturnType",                                 "string",                                    "typeof(Foo).GetMethod(nameof(this.ToString)).ReturnType")]
        public void TryGet(string expression, string expected, string expectedSource)
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
        }

        public class Baz<T>
        {
        }
    }
}".AssertReplace("typeof(Foo)", expression);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindExpression(expression);
            var context = new SyntaxNodeAnalysisContext(null, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true,           Type.TryGet(node, context, out var type, out var source));
            Assert.AreEqual(expected,       type.ToDisplayString());
            Assert.AreEqual(expectedSource, source.ToString());
        }
    }
}
