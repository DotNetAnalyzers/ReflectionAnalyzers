﻿namespace ReflectionAnalyzers.Tests.REFL009MemberCantBeFoundTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static partial class Valid
{
    private static readonly GetXAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL009MemberCannotBeFound;

    [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
    [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    public static void AggregateExceptionGetInnerExceptionCount(string call)
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
            var member = typeof(AggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
    public static void SubclassAggregateExceptionGetInnerExceptionCount(string call)
    {
        var customAggregateException = @"
namespace N
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        private readonly int f = 1;

        public int M() => this.f;
    }
}";
        var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(CustomAggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

        RoslynAssert.Valid(Analyzer, Descriptor, customAggregateException, code);
    }

    [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public)")]
    [TestCase("GetNestedType(\"Generic`1\", BindingFlags.Public)")]
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

        public class Generic<T>
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

    [TestCase("GetProperty(\"Item\")")]
    [TestCase("GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    public static void Indexer(string call)
    {
        var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public PropertyInfo? Get() => typeof(C).GetProperty(""Item"");

        public int this[int p1] => 0;
    }
}".AssertReplace("GetProperty(\"Item\")", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("GetProperty(\"Bar\")")]
    [TestCase("GetProperty(\"Bar\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
    public static void NamedIndexer(string call)
    {
        var code = @"
namespace N
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        public PropertyInfo? Get() => typeof(C).GetProperty(""Bar"");

        [IndexerName(""Bar"")]
        public int this[int p1] => 0;
    }
}".AssertReplace("GetProperty(\"Bar\")", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("property == null")]
    [TestCase("property != null")]
    [TestCase("property is null")]
    public static void GetMissingPropertyThenNullCheck(string check)
    {
        var code = @"
namespace N
{
    public class C
    {
        public C(C c)
        {
            var property = c.GetType().GetProperty(""P"");
            if (property != null)
            {
            }
        }
    }
}".AssertReplace("property != null", check);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void GetMissingPropertyConditional()
    {
        var code = @"
namespace N
{
    public class C
    {
        public C(C c)
        {
            var value = c.GetType().GetProperty(""P"")?.SetMethod?.Invoke(c, null);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void GetMissingPropertyIsPattern()
    {
        var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public C(C c)
        {
            if (c.GetType().GetProperty(""P"") is PropertyInfo property)
            {
            }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
}
