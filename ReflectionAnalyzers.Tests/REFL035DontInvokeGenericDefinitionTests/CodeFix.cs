namespace ReflectionAnalyzers.Tests.REFL035DontInvokeGenericDefinitionTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly CodeFixProvider Fix = new CallMakeGenericMethodFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL035DontInvokeGenericDefinition.Descriptor);

        [Test]
        public static void ParameterlessGeneric()
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public static void M<T>() => typeof(C).GetMethod(nameof(C.M)).↓Invoke(null, null);
    }
}";

            var message = "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public static void WithAccessibleParameterSingleLine()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public T Id<T>(T value) => value;

        public int Call() => (int)typeof(C).GetMethod(nameof(this.Id)).↓Invoke(null, new object[] { 1 });
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public T Id<T>(T value) => value;

        public int Call() => (int)typeof(C).GetMethod(nameof(this.Id)).MakeGenericMethod(typeof(int)).Invoke(null, new object[] { 1 });
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
