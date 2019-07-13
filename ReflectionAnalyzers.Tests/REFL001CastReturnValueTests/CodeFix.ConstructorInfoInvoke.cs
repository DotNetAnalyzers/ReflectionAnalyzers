namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ConstructorInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

            [Test]
            public static void AssigningLocal()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(int i)
        {
            var value = ↓typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(int i)
        {
            var value = (C)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }

            [Test]
            public static void Walk()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(int i)
        {
            var info = typeof(C).GetConstructor(new[] { typeof(int) });
            var value = ↓info.Invoke(new object[] { 1 });
        }
    }
}";

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
