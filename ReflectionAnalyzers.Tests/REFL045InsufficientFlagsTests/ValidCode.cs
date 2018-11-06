namespace ReflectionAnalyzers.Tests.REFL045InsufficientFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL045InsufficientFlags.Descriptor;

        [TestCase("GetField(\"F\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
        [TestCase("GetEvent(\"F\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetProperty(\"P\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetNestedType(\"M\", BindingFlags.Public)")]
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

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
