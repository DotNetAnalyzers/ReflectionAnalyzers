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
            [TestCase("CreateInstance<Foo>()")]
            public void Generic(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static void Bar<T>()
        {
            var foo = Activator.CreateInstance<T>();
        }
    }
}".AssertReplace("CreateInstance<T>()", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
