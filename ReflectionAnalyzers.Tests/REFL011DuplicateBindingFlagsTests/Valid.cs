namespace ReflectionAnalyzers.Tests.REFL011DuplicateBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly BindingFlagsAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL011DuplicateBindingFlags;

        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | System.Reflection.BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetMethod(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo Get() => typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        public int M() => 0;
    }
}".AssertReplace("GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(nameof(this.M), Public | Static | DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.M), Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(this.M), Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.M), Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.Public | System.Reflection.BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.M), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetMethodUsingStatic(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class C
    {
        public MethodInfo Get() => typeof(C).GetMethod(nameof(this.M), Public | Static | DeclaredOnly);

        public int M() => (int)Static;
    }
}".AssertReplace("GetMethod(nameof(this.M), Public | Static | DeclaredOnly)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
