namespace ReflectionAnalyzers.Tests.REFL019NoMemberMatchesTheTypesTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly GetXAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL019NoMemberMatchesTypes;

    [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)")]
    [TestCase("typeof(C).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
    [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Static |BindingFlags.Public | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)")]
    [TestCase("typeof(C).GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(C).GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(IConvertible).GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    [TestCase("typeof(IConvertible).GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(IFormatProvider) }, null)")]
    public static void GetMethod(string call)
    {
        var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MemberInfo? Get(Type unused) => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
    [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
    public static void GetMethodOneParameter(string call)
    {
        var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public object? Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static int Static(int i) => i;

        public int Public(int i) => i;

        private int Private(int i) => i;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
    [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
    public static void GetMethodOneParameterOverloadResolution(string call)
    {
        var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public object? Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("typeof(C).GetConstructor(new[] { typeof(int) })")]
    [TestCase("typeof(C).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
    [TestCase("typeof(int[]).GetConstructor(new[] { typeof(int) })")]
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

        public MemberInfo? Get(Type unused) => typeof(C).GetConstructor(new[] { typeof(int) });
    }
}".AssertReplace("typeof(C).GetConstructor(new[] { typeof(int) })", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("GetConstructor(Type.EmptyTypes)")]
    [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
    [TestCase("GetConstructor(new[] { typeof(int) })")]
    [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
    [TestCase("GetConstructor(new[] { typeof(double) })")]
    [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null)")]
    public static void GetConstructorWhenOverloaded(string call)
    {
        var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
        }

        public C(int value)
        {
        }

        public C(double value)
        {
        }

        public MemberInfo? Get(Type unused) => typeof(C).GetConstructor(Type.EmptyTypes);
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes)", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
}
