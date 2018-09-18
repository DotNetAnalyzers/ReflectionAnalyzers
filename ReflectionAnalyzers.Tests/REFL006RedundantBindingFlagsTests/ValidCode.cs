namespace ReflectionAnalyzers.Tests.REFL006RedundantBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL006");

        [TestCase("GetMethod(Static, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(ReferenceEquals, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(this.Public, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(this.ToString, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(this.GetHashCode, BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(this.Private, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void GetMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("GetMethod(nameof(this.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
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
        public void GetMethodUnknownType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo(Type type)
        {
            var methodInfo = type.GetMethod(""Bar"");
        }
    }
}".AssertReplace("GetMethod(\"Bar\")", call);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetNestedType(nameof(PublicStatic))")]
        [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(Public))")]
        [TestCase("GetNestedType(nameof(Public), BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(Public), BindingFlags.Public | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic)")]
        [TestCase("GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic | BindingFlags.DeclaredOnly)")]
        [TestCase("GetNestedType(nameof(Private), BindingFlags.NonPublic)")]
        [TestCase("GetNestedType(nameof(Private), BindingFlags.NonPublic | BindingFlags.DeclaredOnly)")]
        public void GetNestedType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(nameof(Public), BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

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
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
