namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly CodeFixProvider Fix = new SuggestTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL037TypeDoesNotExits);

        [TestCase("Type.GetType(↓\"Int32\")",              "Type.GetType(\"System.Int32\")")]
        [TestCase("Type.GetType(↓\"Int32\", true)",        "Type.GetType(\"System.Int32\", true)")]
        [TestCase("Type.GetType(↓\"Int32\", true, true)",  "Type.GetType(\"System.Int32\", true, true)")]
        [TestCase("Type.GetType(↓\"Nullable`1\")",         "Type.GetType(\"System.Nullable`1\")")]
        [TestCase("Type.GetType(↓\"IComparable\")",        "Type.GetType(\"System.IComparable\")")]
        [TestCase("Type.GetType(↓\"IComparable`1\")",      "Type.GetType(\"System.IComparable`1\")")]
        //[TestCase("Type.GetType(↓\"IEnumerable`1\")",      "Type.GetType(\"System.Collections.Generic.IEnumerable`1\")")]
        //[TestCase("Type.GetType(↓\"AppContextSwitches\")", "Type.GetType(\"System.AppContextSwitches\")")]
        public static void TypeGetTypeWithFix(string type, string fixedType)
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(↓""Int32"");
    }
}".AssertReplace("Type.GetType(↓\"Int32\")", type);

            var after = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""System.Int32"");
    }
}".AssertReplace("Type.GetType(\"System.Int32\")", fixedType);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("typeof(int).Assembly.GetType(↓\"Int32\")",                                                                             "typeof(int).Assembly.GetType(\"System.Int32\")")]
        [TestCase("typeof(System.Linq.Expressions.BinaryExpression).Assembly.GetType(\"BinaryExpression\")",                              "typeof(System.Linq.Expressions.BinaryExpression).Assembly.GetType(\"System.Linq.Expressions.BinaryExpression\")")]
        [TestCase("typeof(System.Windows.Controls.AdornedElementPlaceholder).Assembly.GetType(\"TemplatedAdorner\", throwOnError: true)", "typeof(System.Windows.Controls.AdornedElementPlaceholder).Assembly.GetType(\"MS.Internal.Controls.TemplatedAdorner\", throwOnError: true)")]
        public static void AssemblyGetTypeWithFix(string type, string fixedType)
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(↓""Int32"");
    }
}".AssertReplace("typeof(int).Assembly.GetType(↓\"Int32\")", type);

            var after = @"
namespace N
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(""System.Int32"");
    }
}".AssertReplace("typeof(int).Assembly.GetType(\"System.Int32\")", fixedType);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
