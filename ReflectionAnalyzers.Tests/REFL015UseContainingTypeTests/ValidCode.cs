namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL015");

        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
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

        public static event EventHandler PublicStaticEvent;

        public static int PublicStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;
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
