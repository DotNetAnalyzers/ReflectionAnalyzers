namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTypeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL037TypeDoesNotExits.Descriptor;

        [TestCase("System.Int32")]
        [TestCase("System.AppContextSwitches")]
        [TestCase("System.Nullable`1[System.Int32]")]
        [TestCase("System.Nullable`1[[System.Int32]]")]
        [TestCase("System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")]
        [TestCase("System.Collections.Generic.Dictionary`2[System.Int32,System.String]")]
        [TestCase("System.Collections.Generic.Dictionary`2[[System.Int32], [System.String]]")]
        [TestCase("System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")]
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

        [TestCase("system.int32")]
        public void TypeGetTypeIgnoreCase(string type)
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

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
