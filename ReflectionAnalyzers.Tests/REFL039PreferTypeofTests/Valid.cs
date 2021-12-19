namespace ReflectionAnalyzers.Tests.REFL039PreferTypeofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetTypeAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL039PreferTypeof;

        [Test]
        public static void AnonymousType()
        {
            var code = @"
namespace ValidCode
{
    using System.Reflection;

    class C
    {
        object? M()
        {
            var anon = new { C = 1 };
            return anon.GetType().GetProperty(nameof(anon.C), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void AnonymousTypePropertyWithFlags()
        {
            var code = @"
namespace ValidCode
{
    using System.Reflection;

    class C
    {
        object? M()
        {
            var anon = new { C = 1 };
            return anon.GetType().GetProperty(nameof(anon.C), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void MulticastDelegate()
        {
            var code = @"
namespace ValidCode
{
    using System;

    class C
    {
        public object Get(MulticastDelegate m) => m.GetType();
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void Delegate()
        {
            var code = @"
namespace ValidCode
{
    using System;

    class C
    {
        public object? Get(Delegate m) => m.GetType();
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("pi.GetValue(null).GetType()")]
        [TestCase("pi.GetValue(null)?.GetType()")]
        public static void UnknownType(string call)
        {
            var code = @"
#pragma warning disable CS8602
namespace ValidCode
{
    using System;
    using System.Reflection;

    class C
    {
        public Type? Get(PropertyInfo pi) => pi.GetValue(null).GetType();
    }
}".AssertReplace("pi.GetValue(null).GetType()", call);

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
