namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL003");

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void AggregateExceptionGetInnerExceptionCount(string call)
        {
            var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        public void SubclassAggregateExceptionGetInnerExceptionCount(string call)
        {
            var exception = @"
namespace RoslynSandbox
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }
}";
            var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(CustomAggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, exception, code);
        }

        [TestCase("GetNestedType(nameof(PublicStatic), BindingFlags.Public)")]
        [TestCase("GetNestedType(\"Generic`1\", BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(Public), BindingFlags.Public)")]
        [TestCase("GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic)")]
        [TestCase("GetNestedType(nameof(Private), BindingFlags.NonPublic)")]
        public void GetNestedType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(nameof(Public), BindingFlags.Public);
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
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
