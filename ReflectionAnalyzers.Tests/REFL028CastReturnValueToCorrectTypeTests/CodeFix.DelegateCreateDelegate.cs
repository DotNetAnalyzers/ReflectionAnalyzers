namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public partial class CodeFix
    {
        public class DelegateCreateDelegate
        {
            private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL028CastReturnValueToCorrectType.Descriptor);

            [Test]
            public void WhenCastingToWrongType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var value = (↓Func<double>)Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }
        }
    }
}
