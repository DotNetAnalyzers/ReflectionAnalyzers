namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ActivatorCreateInstance
        {
            private static readonly ActivatorAnalyzer Analyzer = new();
            private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL001CastReturnValue;

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
