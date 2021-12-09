namespace ReflectionAnalyzers.Tests.REFL019NoMemberMatchesTheTypesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL019NoMemberMatchesTypes);

        [TestCase("GetConstructor(↓Type.EmptyTypes)")]
        [TestCase("GetConstructor(↓Array.Empty<Type>())")]
        [TestCase("GetConstructor(↓new Type[0])")]
        [TestCase("GetConstructor(↓new Type[1] { typeof(double) })")]
        [TestCase("GetConstructor(↓new Type[] { typeof(double) })")]
        [TestCase("GetConstructor(↓new[] { typeof(double) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, ↓Type.EmptyTypes, null)")]
        public static void GetConstructor(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C(int value)
        {
        }

        public MemberInfo Get(Type unused) => typeof(C).GetConstructor(↓Type.EmptyTypes);
    }
}".AssertReplace("GetConstructor(↓Type.EmptyTypes)", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)")]
        [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)")]
        [TestCase("typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)")]
        public static void GetMethodNoParameters(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MemberInfo Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null);

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(int) }, null)", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(double) }, null)")]
        [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(double) }, null)")]
        public static void GetMethodOneParameter(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MemberInfo Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(double) }, null);

        public static int Static(int i) => i;

        public int Public(int i) => i;

        private int Private(int i) => i;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, ↓new[] { typeof(double) }, null)", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(IEnumerable<int>) })")]
        [TestCase("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int), typeof(IEnumerable<int>) })")]
        public static void OverloadResolution(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class C
    {
        public MemberInfo Get => typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int), typeof(IEnumerable<int>) });
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int), typeof(IEnumerable<int>) })", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
