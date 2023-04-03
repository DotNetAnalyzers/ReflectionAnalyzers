namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class Diagnostics
{
    public static class GetMethod
    {
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.Instance), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void PublicPrivateOverloads(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod↓(nameof(this.ToString));
        }

        public static double Static(int value) => value;

        public int Instance(int value) => value;

        private static double Static(double value) => value;

        private double Instance(double value) => value;
    }
}".AssertReplace("GetMethod↓(nameof(this.ToString))", call);
            var message = "More than one member is matching the criteria";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetMethod↓(nameof(Static))")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.Instance))")]
        [TestCase("GetMethod↓(nameof(this.Instance), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void PublicOverloads(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo? M() => typeof(C).GetMethod↓(nameof(this.ToString));

        public static double Static(int value) => value;

        public static double Static(double value) => value;

        public int Instance(int value) => value;

        public double Instance(double value) => value;
    }
}".AssertReplace("GetMethod↓(nameof(this.ToString))", call);
            var message = "More than one member is matching the criteria";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetMethod↓(\"op_Explicit\")")]
        [TestCase("GetMethod↓(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static)")]
        public static void OverloadedOperatorExplicit(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public static explicit operator int(C c) => default;

        public static explicit operator C?(int c) => default;

        public MethodInfo? M() => typeof(C).GetMethod↓(""op_Explicit"");
    }
}".AssertReplace("GetMethod↓(\"op_Explicit\")", call);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        public static void OverloadResolution(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public static object? Get() => typeof(C).GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static IComparable Static(IComparable i) => i;

        public static IFormattable Static(IFormattable i) => i;
    }
}".AssertReplace("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
