namespace ReflectionAnalyzers.Tests.REFL045InsufficientFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL045InsufficientFlags;

        [TestCase("GetField(\"F\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
        [TestCase("GetEvent(\"F\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetProperty(\"P\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"M\", BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetNestedType(\"M\", BindingFlags.Public)")]
        public static void GetX(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Get(Type type) => type.GetMethod(""M"", ↓BindingFlags.Public);
    }
}".AssertReplace("GetMethod(\"M\", ↓BindingFlags.Public)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
