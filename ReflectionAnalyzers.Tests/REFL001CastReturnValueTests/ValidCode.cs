namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL001CastReturnValue.Descriptor;

            [TestCase("CreateInstance<T>()")]
            [TestCase("CreateInstance<C>()")]
            public void Generic(string call)
            {
                var code = @"
namespace RoslynSandbox
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
