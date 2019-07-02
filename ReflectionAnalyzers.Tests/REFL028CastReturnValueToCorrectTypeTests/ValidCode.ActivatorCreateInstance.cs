namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        public static class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL028CastReturnValueToCorrectType.Descriptor;

            [TestCase("(C)")]
            [TestCase("(IDisposable)")]
            public static void WhenCasting(string cast)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public sealed class C : IDisposable
    {
        public C()
        {
            var foo = (C)Activator.CreateInstance(typeof(C));
        }

        public void Dispose()
        {
        }
    }
}".AssertReplace("(C)", cast);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void WhenUnknown()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Bar(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void WhenUnconstrainedGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Bar<T>() => (T)Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
