namespace ReflectionAnalyzers.Tests.REFL038PreferRunClassConstructorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseRunClassConstructorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL038PreferRunClassConstructor.Descriptor);

        [Test]
        public void WhenInvokingStatic()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        static C()
        {
        }

        public static void M()
        {
            typeof(C).GetConstructor(BindingFlags.NonPublic| BindingFlags.Static, null, Type.EmptyTypes, null).Invoke(null, null);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        static C()
        {
        }

        public static void M()
        {
            RuntimeHelpers.RunClassConstructor(typeof(C).TypeHandle);
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
