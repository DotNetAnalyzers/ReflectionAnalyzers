namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class TypeTests
    {
        [TestCase("typeof(Foo)",                                                                                                              "RoslynSandbox.Foo",                         "typeof(Foo)")]
        [TestCase("new Foo().GetType()",                                                                                                      "RoslynSandbox.Foo",                         "new Foo().GetType()")]
        [TestCase("foo.GetType()",                                                                                                            "RoslynSandbox.Foo",                         "foo.GetType()")]
        [TestCase("this.GetType()",                                                                                                           "RoslynSandbox.Foo",                         "this.GetType()")]
        [TestCase("GetType()",                                                                                                                "RoslynSandbox.Foo",                         "GetType()")]
        [TestCase("typeof(Foo).GetNestedType(\"Baz`1\")",                                                                                     "RoslynSandbox.Foo.Baz<T>",                  "typeof(Foo).GetNestedType(\"Baz`1\")")]
        [TestCase("typeof(Foo).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))",                                                        "RoslynSandbox.Foo.Baz<int>",                "typeof(Foo).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))")]
        [TestCase("typeof(Foo.Baz<int>).GetGenericTypeDefinition()",                                                                          "RoslynSandbox.Foo.Baz<T>",                  "typeof(Foo.Baz<int>).GetGenericTypeDefinition()")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")",                                                                        "int",                                       "typeof(string).Assembly.GetType(\"System.Int32\")")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\", true)",                                                                  "int",                                       "typeof(string).Assembly.GetType(\"System.Int32\", true)")]
        [TestCase("typeof(string).Assembly.GetType(\"system.int32\", true, true)",                                                            "int",                                       "typeof(string).Assembly.GetType(\"system.int32\", true, true)")]
        [TestCase("Assembly.Load(\"mscorlib\").GetType(\"System.Int32\")",                                                                    "int",                                       "Assembly.Load(\"mscorlib\").GetType(\"System.Int32\")")]
        [TestCase("Assembly.Load(\"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\").GetType(\"System.Int32\")", "int",                                       "Assembly.Load(\"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\").GetType(\"System.Int32\")")]
        [TestCase("new Exception().GetType()",                                                                                                "System.Exception",                          "new Exception().GetType()")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")",                                  "System.Collections.Generic.IEnumerable<T>", "typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("typeof(Foo).GetField(nameof(Foo.Field)).FieldType",                                                                        "int",                                       "typeof(Foo).GetField(nameof(Foo.Field)).FieldType")]
        [TestCase("typeof(Foo).GetProperty(nameof(Foo.Property)).PropertyType",                                                               "int",                                       "typeof(Foo).GetProperty(nameof(Foo.Property)).PropertyType")]
        [TestCase("Type.GetType(\"System.Int32\")",                                                                                           "int",                                       "Type.GetType(\"System.Int32\")")]
        [TestCase("Type.GetType(\"System.Int32\", true)",                                                                                     "int",                                       "Type.GetType(\"System.Int32\", true)")]
        [TestCase("Type.GetType(\"System.Int32\", false)",                                                                                    "int",                                       "Type.GetType(\"System.Int32\", false)")]
        [TestCase("Type.GetType(\"System.Int32\", true, true)",                                                                               "int",                                       "Type.GetType(\"System.Int32\", true, true)")]
        [TestCase("Type.GetType(\"System.Int32\", true, false)",                                                                              "int",                                       "Type.GetType(\"System.Int32\", true, false)")]
        [TestCase("Type.GetType(\"system.int32\", true, true)",                                                                               "int",                                       "Type.GetType(\"system.int32\", true, true)")]
        public void TryGet(string expression, string expected, string expectedSource)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Foo
    {
        public readonly int Field;

        public Foo(Foo foo)
        {
            var type = typeof(Foo);
        }

        public int Property { get; }

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
