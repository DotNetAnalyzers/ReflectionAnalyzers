namespace ReflectionAnalyzers.Tests.REFL018ExplicitImplementationTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly UseContainingTypeFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL018ExplicitImplementation);

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitImplementation(string call)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    sealed class C : IDisposable
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(IDisposable.Dispose));
        }

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    sealed class C : IDisposable
    {
        public C()
        {
            var methodInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        }

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            var message = "Dispose is explicitly implemented";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("GetMethod(\"ToBoolean\")")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("GetMethod(\"ToBoolean\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"ToBoolean\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitImplementationPublicNotInSource(string call)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var methodInfo = typeof(string).GetMethod(""ToBoolean"");
        }
    }
}".AssertReplace("GetMethod(\"ToBoolean\")", call);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var methodInfo = typeof(IConvertible).GetMethod(""ToBoolean"");
        }
    }
}".AssertReplace("GetMethod(\"ToBoolean\")", call);

            var message = "ToBoolean is explicitly implemented";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }
    }
}
