namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL001CastReturnValue.Descriptor;

            [TestCase("CreateInstance<T>()")]
            [TestCase("CreateInstance<C>()")]
            public static void Generic(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object  M<T>() => Activator.CreateInstance<T>();
    }
}".AssertReplace("CreateInstance<T>()", call);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
