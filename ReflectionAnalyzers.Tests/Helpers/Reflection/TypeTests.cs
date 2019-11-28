namespace ReflectionAnalyzers.Tests.Helpers.Reflection
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using Type = ReflectionAnalyzers.Type;

    public static class TypeTests
    {
        [TestCase("typeof(C)", "N.C", "typeof(C)")]
        [TestCase("new C().GetType()", "N.C", "new C().GetType()")]
        [TestCase("foo.GetType()", "N.C", "foo.GetType()")]
        [TestCase("foo?.GetType()", "N.C", "foo?.GetType()")]
        [TestCase("nullableInt.GetType()", "int", "nullableInt.GetType()")]
        [TestCase("this.GetType()", "N.C", "this.GetType()")]
        [TestCase("GetType()", "N.C", "GetType()")]
        [TestCase("typeof(C).GetNestedType(\"Baz`1\")", "N.C.Baz<T>", "typeof(C).GetNestedType(\"Baz`1\")")]
        [TestCase("typeof(C).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))", "N.C.Baz<int>", "typeof(C).GetNestedType(\"Baz`1\").MakeGenericType(typeof(int))")]
        [TestCase("typeof(C.Baz<int>).GetGenericTypeDefinition()", "N.C.Baz<T>", "typeof(C.Baz<int>).GetGenericTypeDefinition()")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\")", "int", "typeof(string).Assembly.GetType(\"System.Int32\")")]
        [TestCase("typeof(string).Assembly.GetType(\"System.Int32\", true)", "int", "typeof(string).Assembly.GetType(\"System.Int32\", true)")]
        [TestCase("typeof(string).Assembly.GetType(\"system.int32\", true, true)", "int", "typeof(string).Assembly.GetType(\"system.int32\", true, true)")]
        [TestCase("new Exception().GetType()", "System.Exception", "new Exception().GetType()")]
        [TestCase("typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")", "System.Collections.Generic.IEnumerable<T>", "typeof(IEnumerable<int>).Assembly.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("typeof(C).GetField(nameof(C.Field)).FieldType", "int", "typeof(C).GetField(nameof(C.Field)).FieldType")]
        [TestCase("typeof(C).GetProperty(nameof(C.Property)).PropertyType", "int", "typeof(C).GetProperty(nameof(C.Property)).PropertyType")]
        [TestCase("Type.GetType(\"N.C\")", "N.C", "Type.GetType(\"N.C\")")]
        [TestCase("Type.GetType(\"N.C+Nested\")", "N.C.Nested", "Type.GetType(\"N.C+Nested\")")]
        [TestCase("Type.GetType(\"System.Int32\")", "int", "Type.GetType(\"System.Int32\")")]
        [TestCase("Type.GetType(\"System.Collections.Generic.KeyValuePair`2[System.Int32,System.String]\")", "System.Collections.Generic.KeyValuePair<int, string>", "Type.GetType(\"System.Collections.Generic.KeyValuePair`2[System.Int32,System.String]\")")]
        [TestCase("Type.GetType(\"System.Int32\", true)", "int", "Type.GetType(\"System.Int32\", true)")]
        [TestCase("Type.GetType(\"System.Int32\", false)", "int", "Type.GetType(\"System.Int32\", false)")]
        [TestCase("Type.GetType(\"System.Int32\", true, true)", "int", "Type.GetType(\"System.Int32\", true, true)")]
        [TestCase("Type.GetType(\"System.Int32\", true, false)", "int", "Type.GetType(\"System.Int32\", true, false)")]
        public static void TryGet(string expression, string expected, string expectedSource)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class C
    {
        public readonly int Field;

        public C(C foo, int? nullableInt)
        {
            var type = typeof(C);
        }

        public int Property { get; }

        public class Baz<T>
        {
        }

        public class Nested { }
    }
}".AssertReplace("typeof(C)", expression);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindExpression(expression);
            Assert.AreEqual(true, Type.TryGet(node, semanticModel, CancellationToken.None, out var type, out var source));
            Assert.AreEqual(expected, type.ToDisplayString());
            Assert.AreEqual(expectedSource, source.ToString());
        }

        [TestCase("field")]
        [TestCase("this.field")]
        [TestCase("local")]
        [TestCase("CalculatedProperty")]
        [TestCase("GetOnlyProperty")]
        [TestCase("this.GetOnlyProperty")]
        [TestCase("Method()")]
        public static void TryGetWalked(string expression)
        {
            var code = @"
namespace N
{
    class C
    {
        public readonly int field = typeof(int);

        public C()
        {
            var local = typeof(int);
            _ = local.ToString();
        }

        public Type CalculatedProperty => typeof(int);

        public Type GetOnlyProperty { get; } = typeof(int);

        public static Type Method() => typeof(int);
    }
}".AssertReplace("local.ToString()", $"{expression}.ToString()");
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = ((MemberAccessExpressionSyntax)syntaxTree.FindInvocation("ToString()").Expression).Expression;
            Assert.AreEqual(true, Type.TryGet(node, semanticModel, CancellationToken.None, out var type, out var source));
            Assert.AreEqual("int", type.ToDisplayString());
            Assert.AreEqual("typeof(int)", source.ToString());
        }

        [TestCase("Assembly.Load(\"mscorlib\").GetType(\"System.Int32\")")]
        [TestCase("Assembly.Load(\"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\").GetType(\"System.Int32\")")]
        public static void TryGetAssemblyLoad(string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class C
    {
        public readonly int Field;

        public C(C foo)
        {
            var type = Assembly.Load(""mscorlib"").GetType(""System.Int32"");
        }

        public int Property { get; }

        public class Baz<T>
        {
        }

        public class Nested { }
    }
}".AssertReplace("Assembly.Load(\"mscorlib\").GetType(\"System.Int32\")", expression);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var node = syntaxTree.FindExpression(expression);
            Assert.AreEqual(false, Type.TryGet(node, semanticModel, CancellationToken.None, out _, out _));
        }
    }
}
