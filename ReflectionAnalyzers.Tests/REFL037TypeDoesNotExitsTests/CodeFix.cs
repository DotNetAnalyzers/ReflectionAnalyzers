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

        [TestCase("Type.GetType(↓\"Int32\")",              "Type.GetType(\"System.Int32\")")]
        [TestCase("Type.GetType(↓\"Int32\", true)",        "Type.GetType(\"System.Int32\", true)")]
        [TestCase("Type.GetType(↓\"Int32\", true, true)",  "Type.GetType(\"System.Int32\", true, true)")]
        [TestCase("Type.GetType(↓\"Wrong.Int32\")",        "Type.GetType(\"System.Int32\")")]
        [TestCase("Type.GetType(↓\"Nullable`1\")",         "Type.GetType(\"System.Nullable`1\")")]
        [TestCase("Type.GetType(↓\"IComparable\")",        "Type.GetType(\"System.IComparable\")")]
        [TestCase("Type.GetType(↓\"IEnumerable`1\")",      "Type.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        [TestCase("Type.GetType(↓\"AppContextSwitches\")", "Type.GetType(\"System.AppContextSwitches\")")]
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
}".AssertReplace("Type.GetType(↓\"Int32\")", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""System.Int32"");
    }
}".AssertReplace("Type.GetType(\"System.Int32\")", fixedType);

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

        [TestCase("typeof(int).Assembly.GetType(↓\"Int32\")", "typeof(int).Assembly.GetType(\"System.Int32\")")]
        public void AssemblyGetTypeWithFix(string type, string fixedType)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(↓""Int32"");
    }
}".AssertReplace("typeof(int).Assembly.GetType(↓\"Int32\")", type);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(""System.Int32"");
    }
}".AssertReplace("typeof(int).Assembly.GetType(\"System.Int32\")", fixedType);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("typeof(int).Assembly.GetType(↓\"MISSING\")")]
        public void AssemblyGetTypeNoFix(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(↓""MISSING"");
    }
}".AssertReplace("MISSING", type);

            AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }
    }
}
