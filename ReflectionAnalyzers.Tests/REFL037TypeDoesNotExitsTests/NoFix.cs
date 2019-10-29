namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class NoFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new SuggestTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL037TypeDoesNotExits.Descriptor);

        [TestCase("MISSING")]
        // [TestCase("RoslynSandbox.MISSING")]
        public static void TypeGetTypeNoFix(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(↓""MISSING"");
    }
}".AssertReplace("MISSING", type);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).Assembly.GetType(↓\"MISSING\")")]
        [TestCase("typeof(C).Assembly.GetType(↓\"RoslynSandbox.MISSING\")")]
        public static void AssemblyGetTypeNoFix(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(C).Assembly.GetType(↓""MISSING"");
    }
}".AssertReplace("typeof(C).Assembly.GetType(↓\"MISSING\")", call);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }
    }
}
