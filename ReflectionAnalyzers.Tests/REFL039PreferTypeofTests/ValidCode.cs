namespace ReflectionAnalyzers.Tests.REFL039PreferTypeofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL039PreferTypeof.Descriptor;

        [Test]
        public void AnonymousType()
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
            var anon = new { Foo = 1 };
            return anon.GetType().GetProperty(nameof(anon.Foo), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void MulticastDelegate()
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

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void Delegate()
        {
            var code = @"
// ReSharper disable All
namespace ValidCode
{
    using System;

    class C
    {
        object M(Delegate m) => m.GetType();
    }
}";

            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
