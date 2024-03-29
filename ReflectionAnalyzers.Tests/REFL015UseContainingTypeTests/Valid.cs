﻿namespace ReflectionAnalyzers.Tests.REFL015UseContainingTypeTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly GetXAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL015UseContainingType;

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
    public static void GetMember(string call)
    {
        var code = @"
#pragma warning disable CS8618
namespace N
{
    using System;
    using System.Reflection;

    public class CBase
    {
        public int PublicStaticField;

        internal static int InternalStaticField = 1;

        private static int PrivateStaticField = 2;

        public static event EventHandler PublicStaticEvent;

        internal static event EventHandler InternalStaticEvent;

        private static event EventHandler PrivateStaticEvent;

        public static int PublicStaticProperty => PrivateStaticField;

        internal static int InternalStaticProperty { get; set; }

        private static int PrivateStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;

        internal static int InternalStaticMethod() => 0;

        private static int PrivateStaticMethod() => 0;

        public static void M()
        {
            PublicStaticEvent?.Invoke(null, EventArgs.Empty);
            InternalStaticEvent?.Invoke(null, EventArgs.Empty);
            PrivateStaticEvent?.Invoke(null, EventArgs.Empty);
        }
    }

    public class C : CBase
    {
        public MemberInfo? Get() => typeof(C).GetEvent(nameof(CBase.PublicStaticEvent));
    }
}".AssertReplace("typeof(C).GetEvent(nameof(CBase.PublicStaticEvent))", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void InternalStaticFieldInBase()
    {
        var baseCode = @"
namespace N
{
    class B
    {
        internal static readonly int field = 1;
    }
}";
        var code = @"
namespace N
{
    using System.Reflection;

     class C : B
    {
        public object? Get => typeof(C).GetField(nameof(B.field), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, baseCode, code);
    }
}
