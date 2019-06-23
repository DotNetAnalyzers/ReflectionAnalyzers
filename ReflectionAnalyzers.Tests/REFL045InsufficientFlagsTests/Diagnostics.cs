namespace ReflectionAnalyzers.Tests.REFL045InsufficientFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL045InsufficientFlags.Descriptor);

        [TestCase("GetField(\"F\", ↓BindingFlags.Instance)")]
        [TestCase("GetConstructor(↓BindingFlags.Static, null, Type.EmptyTypes, null)")]
        [TestCase("GetEvent(\"F\", ↓BindingFlags.Instance)")]
        [TestCase("GetProperty(\"P\", ↓BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", ↓BindingFlags.Public)")]
        [TestCase("GetMethod(\"M\", ↓BindingFlags.Public | BindingFlags.NonPublic)")]
        [TestCase("GetMethod(\"M\", ↓BindingFlags.Static)")]
        [TestCase("GetMethod(\"M\", ↓BindingFlags.Instance)")]
        public void GetX(string call)
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
