namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetMethodAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL005");

        [TestCase("GetMethod(nameof(Static))")]
        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(StaticPublicPrivate))")]
        [TestCase("GetMethod(nameof(StaticPublicPrivate), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod(nameof(StaticPublicPrivate), BindingFlags.NonPublic | BindingFlags.Static)")]
        [TestCase("GetMethod(nameof(StaticPublicPrivate), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(StaticPublicPrivate), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(this.Public))")]
        [TestCase("GetMethod(nameof(this.Public, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase))")]
        [TestCase("GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicPrivate))")]
        [TestCase("GetMethod(nameof(this.PublicPrivate), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.PublicPrivate), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.PublicPrivate), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicPrivate), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.ToString))")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Public)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase)")]
        [TestCase("GetMethod(nameof(this.GetHashCode))")]
        [TestCase("GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.Instance | BindingFlags.NonPublic)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase)")]
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public static int StaticPublicPrivate() => 0;

        public int Public() => 0;

        public int PublicPrivate() => 0;

        public override string ToString() => string.Empty;

        private static int StaticPublicPrivate() => 0;

        private int Private() => 0;

        private int PublicPrivate() => 0;
    }
}".AssertReplace("GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)", call);
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
    }
}
