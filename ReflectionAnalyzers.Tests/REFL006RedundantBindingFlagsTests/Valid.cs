namespace ReflectionAnalyzers.Tests.REFL006RedundantBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL006RedundantBindingFlags;

        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetMethod(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("GetMethod(nameof(this.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"Bar\")")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.NonPublic | BindingFlags.Static)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(\"Bar\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetMethodUnknownType(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo M(Type type) => type.GetMethod(""Bar"");
    }
}".AssertReplace("GetMethod(\"Bar\")", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(string).GetMethod(nameof(string.Compare), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(string).GetMethod(nameof(string.Compare), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)")]
        [TestCase("typeof(string).GetMethod(nameof(string.Compare), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)")]
        [TestCase("typeof(string).GetMethod(nameof(string.EndsWith), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance)")]
        public static void WhenTypeIsNotInSln(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public object M() => typeof(string).GetMethod(nameof(string.Compare), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
    }
}".AssertReplace("typeof(string).GetMethod(nameof(string.Compare), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetNestedType(nameof(PublicStatic))")]
        [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(Public))")]
        [TestCase("GetNestedType(nameof(Public), BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic)")]
        [TestCase("GetNestedType(nameof(Private), BindingFlags.NonPublic)")]
        public static void GetNestedType(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public object M(BindingFlags unused) => typeof(C).GetNestedType(nameof(Public), BindingFlags.Public | BindingFlags.DeclaredOnly);

        public static class PublicStatic
        {
        }

        public class Public
        {
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}".AssertReplace("GetNestedType(nameof(Public), BindingFlags.Public | BindingFlags.DeclaredOnly)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
