namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new SuggestTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL037TypeDoesNotExits.Descriptor);

        [TestCase("Int32", "System.Int32")]
        [TestCase("Wrong.Int32", "System.Int32")]
        [TestCase("Nullable`1", "System.Nullable`1")]
        [TestCase("IComparable", "System.IComparable")]
        [TestCase("IEnumerable`1", "System.Collections.Generic.IEnumerable`1")]
        [TestCase("AppContextSwitches", "System.AppContextSwitches")]
        public void TypeGetTypeWithFix(string type, string fixedType)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(↓""Int32"");
    }
}".AssertReplace("Int32", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""System.Int32"");
    }
}".AssertReplace("System.Int32", fixedType);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("MISSING")]
        public void TypeGetTypeNoFix(string type)
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

            AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }
    }
}
