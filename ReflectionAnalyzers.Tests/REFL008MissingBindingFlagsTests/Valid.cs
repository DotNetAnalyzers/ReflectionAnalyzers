namespace ReflectionAnalyzers.Tests.REFL008MissingBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL008MissingBindingFlags;

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
        [TestCase("typeof(IConvertible).GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)")]
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
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"M\")")]
        [TestCase("GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
        public static void GetMethodFromTypeParameter(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo M<T>(Type unused) => typeof(T).GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance);
    }
}".AssertReplace("GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void GetMethodOverloaded()
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo Get() => typeof(C).GetMethod(nameof(this.M));

        public void M()
        {
        }

        public int M(int i) => i;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"M\")")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod(\"M\", BindingFlags.NonPublic | BindingFlags.Static)")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(\"M\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void GetMethodUnknownType(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo Get(Type type) => type.GetMethod(""M"");
    }
}".AssertReplace("GetMethod(\"M\")", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public)")]
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
        public C()
        {
            var methodInfo = typeof(C).GetNestedType(nameof(Public), BindingFlags.Public);
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
}".AssertReplace("GetNestedType(nameof(Public), BindingFlags.Public)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetProperty(nameof(AggregateException.Message), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetProperty(nameof(AggregateException.Message), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void AggregateExceptionMessage(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(nameof(AggregateException.Message), BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetProperty(nameof(AggregateException.Message), BindingFlags.NonPublic | BindingFlags.Instance)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
