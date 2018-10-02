namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MakeGenericMethod
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL034DontMakeGeneric.Descriptor);

            [Test]
            public void WhenNotGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public static object M() => typeof(C).GetMethod(nameof(M)).↓MakeGenericMethod(typeof(int));
    }
}";
                var message = "RoslynSandbox.C.M() is not a GenericMethodDefinition. MakeGenericMethod may only be called on a method for which MethodBase.IsGenericMethodDefinition is true.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
