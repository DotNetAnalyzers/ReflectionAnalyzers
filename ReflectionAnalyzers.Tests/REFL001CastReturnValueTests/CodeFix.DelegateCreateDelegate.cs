namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public static partial class CodeFix
    {
        public static class DelegateCreateDelegate
        {
            private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

            [Test]
            public static void AssigningLocal()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var value = ↓Delegate.CreateDelegate(
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

            [Test]
            public static void Walk()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = ↓Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
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
            var info = typeof(C).GetMethod(nameof(M));
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
        }

        public static int M() => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }
        }
    }
}
