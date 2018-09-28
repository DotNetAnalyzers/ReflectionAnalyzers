namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class BindingFlagsExtTests
    {
        private static readonly BindingFlags[] Flags = Enum.GetValues(typeof(BindingFlags))
                                                           .Cast<BindingFlags>()
                                                           .ToArray();

        [TestCaseSource(nameof(Flags))]
        public void ToDisplayString(object flags)
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    class Foo
    {
    }
}");
            Assert.AreEqual("BindingFlags." + flags, ((BindingFlags)flags).ToDisplayString(tree.FindClassDeclaration("Foo")));
        }

        [TestCaseSource(nameof(Flags))]
        public void ToDisplayStringUsingStaticInside(object flags)
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using static System.Reflection.BindingFlags;

    class Foo
    {
    }
}");
            Assert.AreEqual(flags.ToString(), ((BindingFlags)flags).ToDisplayString(tree.FindClassDeclaration("Foo")));
        }

        [TestCaseSource(nameof(Flags))]
        public void ToDisplayStringUsingStaticOutside(object flags)
        {
            var tree = CSharpSyntaxTree.ParseText(@"
using static System.Reflection.BindingFlags;

namespace RoslynSandbox
{
    class Foo
    {
    }
}");
            Assert.AreEqual(flags.ToString(), ((BindingFlags)flags).ToDisplayString(tree.FindClassDeclaration("Foo")));
        }
    }
}
