namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public static partial class CodeFix
    {
        public static class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL028CastReturnValueToCorrectType.Descriptor);

            [Test]
            public static void WhenCastingToWrongType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public sealed class C : IDisposable
    {
        public C()
        {
            var foo = (↓string)Activator.CreateInstance(typeof(C));
        }

        public void Dispose()
        {
        }
    }
}";

                var fixedCode = @"
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
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }
        }
    }
}
