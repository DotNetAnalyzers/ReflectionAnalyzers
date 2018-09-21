namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL015");

        [TestCase("typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetField(nameof(FooBase.InternalStaticField), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.InternalStaticEvent), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetProperty(nameof(FooBase.InternalStaticProperty), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetMethod(nameof(FooBase.InternalStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public void GetMember(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class FooBase
    {
        public int PublicStaticField;

        internal static int InternalStaticField;

        private static int PrivateStaticField;

        public static event EventHandler PublicStaticEvent;

        internal static event EventHandler InternalStaticEvent;

        private static event EventHandler PrivateStaticEvent;

        public static int PublicStaticProperty { get; set; }

        internal static int InternalStaticProperty { get; set; }

        private static int PrivateStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;

        internal static int InternalStaticMethod() => 0;

        private static int PrivateStaticMethod() => 0;
    }

    public class Foo : FooBase
    {
        public Foo()
        {
            var member = typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent));
        }
    }
}".AssertReplace("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent))", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
