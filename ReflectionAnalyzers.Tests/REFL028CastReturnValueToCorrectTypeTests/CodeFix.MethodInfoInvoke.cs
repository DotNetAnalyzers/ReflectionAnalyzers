namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL028CastReturnValueToCorrectType.Descriptor);

            [Test]
            public static void WhenCastingToWrongType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => (↓string)typeof(C).GetMethod(nameof(Bar)).Invoke(null, new object[] { 1 });

        public static int Bar(int i) => i;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => (int)typeof(C).GetMethod(nameof(Bar)).Invoke(null, new object[] { 1 });

        public static int Bar(int i) => i;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }
        }
    }
}
