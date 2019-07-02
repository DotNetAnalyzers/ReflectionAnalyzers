namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL037TypeDoesNotExits.Descriptor;

        [TestCase("Missing.Missing")]
        [TestCase("Missing.Missing.Missing")]
        [TestCase("System.Int32")]
        [TestCase("System.AppContextSwitches")]
        [TestCase("System.Nullable`1[System.Int32]")]
        [TestCase("System.Nullable`1[[System.Int32]]")]
        [TestCase("System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")]
        [TestCase("System.Collections.Generic.Dictionary`2[System.Int32,System.String]")]
        [TestCase("System.Collections.Generic.Dictionary`2[[System.Int32], [System.String]]")]
        [TestCase("System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")]
        public static void TypeGetType(string type)
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("system.int32")]
        public static void TypeGetTypeIgnoreCase(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => Type.GetType(""system.int32"", true, true);
    }
}".AssertReplace("system.int32", type);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(int).Assembly.GetType(\"System.Int32\")")]
        [TestCase("typeof(System.Linq.Expressions.BinaryExpression).Assembly.GetType(\"System.Linq.Expressions.BinaryExpression\")")]
        [TestCase("typeof(System.Linq.Expressions.BinaryExpression).Assembly.GetType(\"System.Linq.Expressions.BinaryExpression\", throwOnError: true)")]
        [TestCase("typeof(System.Linq.Expressions.BinaryExpression).Assembly.GetType(\"System.Linq.Expressions.AssignBinaryExpression\", throwOnError: true)")]
        [TestCase("typeof(System.Windows.Controls.AdornedElementPlaceholder).Assembly.GetType(\"MS.Internal.Controls.TemplatedAdorner\", throwOnError: true)")]
        [TestCase("typeof(System.Windows.Forms.FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog\", throwOnError: true)")]
        [TestCase("typeof(System.Windows.Forms.FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog+VistaDialogEvents\", throwOnError: true)")]
        [TestCase("typeof(System.Windows.Forms.DataObject).Assembly.GetType(\"System.Windows.Forms.DataObject+OleConverter\", throwOnError: true)")]
        public static void AssemblyGetTypeFullyQualified(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(int).Assembly.GetType(""System.Int32"");
    }
}".AssertReplace("typeof(int).Assembly.GetType(\"System.Int32\")", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog\")")]
        [TestCase("typeof(FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog\", throwOnError: true)")]
        [TestCase("typeof(FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog+VistaDialogEvents\", throwOnError: true)")]
        [TestCase("typeof(DataObject).Assembly.GetType(\"System.Windows.Forms.DataObject+OleConverter\", throwOnError: true)")]
        public static void AssemblyGetTypeWhenUsing(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Forms;

    public class C
    {
        public static object Get => typeof(FileDialog).Assembly.GetType(""System.Windows.Forms.FileDialog"");
    }
}".AssertReplace("typeof(FileDialog).Assembly.GetType(\"System.Windows.Forms.FileDialog\")", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
