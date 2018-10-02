namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class GetXTests
    {
        [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), new[] { typeof(System.Type), typeof(int) })", "System.Array.CreateInstance(System.Type, int)")]
        [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", "System.Array.CreateInstance(System.Type, int)")]
        [TestCase("typeof(System.Activator).GetMethod(nameof(System.Activator.CreateInstance), new[] { typeof(System.Reflection.TypeInfo) })", "System.Activator.CreateInstance(System.Type)")]
        [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int32) })", "System.Array.GetValue(int)")]
        [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int64) })", "System.Array.GetValue(long)")]
        public void GetMethodOverloadResolutionSuccess(string call, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Foo
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) });
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) })", call);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
            Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
            Assert.AreEqual(expected, reflectedMember.Symbol.ToDisplayString());
        }

        [TestCase("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(string) })")]
        [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(object) })")]
        [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(MemberInfo) })")]
        [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.IFormattable) })")]
        public void GetMethodOverloadResolutionNoMatch(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Foo
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null);
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", call);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
            Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
            Assert.AreEqual(FilterMatch.WrongTypes, reflectedMember.Match);
        }
    }
}
