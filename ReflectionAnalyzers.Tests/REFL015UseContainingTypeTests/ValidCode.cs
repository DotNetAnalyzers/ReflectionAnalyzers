namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL015UseContainingType.Descriptor;

        [TestCase("typeof(C).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetField(nameof(CBase.InternalStaticField), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetEvent(nameof(CBase.InternalStaticEvent), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetProperty(nameof(CBase.InternalStaticProperty), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetMethod(nameof(CBase.InternalStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(CBase).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(CBase).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(CBase).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(CBase).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public void GetMember(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class CBase
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

    public class C : CBase
    {
        public C()
        {
            var member = typeof(C).GetEvent(nameof(CBase.PublicStaticEvent));
        }
    }
}".AssertReplace("typeof(C).GetEvent(nameof(CBase.PublicStaticEvent))", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void InternalStaticFieldInBase()
        {
            var baseCode = @"
namespace RoslynSandbox
{
    class B
    {
        internal static readonly int field;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

     class C : B
    {
        public object Get => typeof(C).GetField(nameof(B.field), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, baseCode, code);
        }
    }
}
