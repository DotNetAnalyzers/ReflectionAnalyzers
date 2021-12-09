namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        public static class ActivatorCreateInstance
        {
            private static readonly ActivatorAnalyzer Analyzer = new();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL028CastReturnValueToCorrectType;

            [TestCase("(C)")]
            [TestCase("(IDisposable)")]
            public static void WhenCasting(string cast)
            {
                var code = @"
namespace N
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
namespace N
{
    using System;

    public class C
    {
        public static object M(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void WhenUnconstrainedGeneric()
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object M<T>() => (T)Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
