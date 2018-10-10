namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new UseFullyQualifiedFix();
        private static readonly DiagnosticDescriptor Descriptor = REFL037TypeDoesNotExits.Descriptor;

        [TestCase("System.Int32")]
        [TestCase("System.AppContextSwitches")]
        public void TypeGetType(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""System.Int32"");
    }
}".AssertReplace("System.Int32", type);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
