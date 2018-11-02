namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL028CastReturnValueToCorrectType.Descriptor;

            [TestCase("(C)")]
            [TestCase("(IDisposable)")]
            public void WhenCasting(string cast)
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
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void WhenUnknown()
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

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void WhenUnconstrainedGeneric()
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

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
