namespace ReflectionAnalyzers.Tests.REFL034DontMakeGenericTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class MakeGenericType
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL034DontMakeGeneric.Descriptor);

            [Test]
            public void WhenNotGeneric()
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).↓MakeGenericType(typeof(int));
    }
}";
                var message = "RoslynSandbox.C is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public void WhenNotGenericDefinition()
            {
                var code = @"
namespace RoslynSandbox
{
    class C<T>
    {
        public object Get => typeof(C<int>).↓MakeGenericType(typeof(int));
    }
}";
                var message = "RoslynSandbox.C<int> is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
