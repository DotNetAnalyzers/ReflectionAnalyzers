namespace ReflectionAnalyzers.Tests.REFL039PreferTypeofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL039PreferTypeof.Descriptor;

        [Test]
        public static void AnonymousType()
        {
            var code = @"
// ReSharper disable All
namespace ValidCode
{
    using System.Reflection;

    class C
    {
        object M()
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
// ReSharper disable All
namespace ValidCode
{
    using System.Reflection;

    class C
    {
        object M()
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
// ReSharper disable All
namespace ValidCode
{
    using System;

    class C
    {
        object M(MulticastDelegate m) => m.GetType();
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
        object M(Delegate m) => m.GetType();
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("pi.GetValue(null).GetType()")]
        [TestCase("pi.GetValue(null)?.GetType()")]
        public static void UnknownType(string call)
        {
            var code = @"
namespace ValidCode
{
    using System;
    using System.Reflection;

    class C
    {
        object M(PropertyInfo pi) => pi.GetValue(null).GetType();
    }
}".AssertReplace("pi.GetValue(null).GetType()", call);

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
