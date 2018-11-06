namespace ReflectionAnalyzers.Tests.REFL045InsufficientFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL045InsufficientFlags.Descriptor);

        [TestCase("GetConstructor(↓BindingFlags.Static, null, Type.EmptyTypes, null)")]
        public void GetMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Get(Type type) => type.GetMethod(""M"", ↓BindingFlags.Public);
    }
}".AssertReplace("GetMethod(\"M\", ↓BindingFlags.Public)", call);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
