namespace ReflectionAnalyzers.Tests.REFL024PreferNullOverEmptyArrayTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly CodeFixProvider Fix = new PreferNullFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL024PreferNullOverEmptyArray.Descriptor);

        [TestCase("Array.Empty<object>()")]
        [TestCase("new object[0]")]
        [TestCase("new object[0] { }")]
        [TestCase("new object[] { }")]
        public static void MemberInfoInvoke(string emptyArray)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        public C(MethodInfo member)
        {
            _ = member.Invoke(null, Array.Empty<object>());
        }
    }
}".AssertReplace("Array.Empty<object>()", emptyArray);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        public C(MethodInfo member)
        {
            _ = member.Invoke(null, null);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
