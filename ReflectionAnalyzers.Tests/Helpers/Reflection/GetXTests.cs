namespace ReflectionAnalyzers.Tests.Helpers.Reflection
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static partial class GetXTests
    {
        [Test]
        public static void GetMethodWrongFlagsWhenNotVisible()
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public object Get => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetMethod");
            Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, semanticModel, CancellationToken.None, out var reflectedMember, out _, out _, out _));
            Assert.AreEqual(FilterMatch.PotentiallyInvisible, reflectedMember.Match);
        }
    }
}
