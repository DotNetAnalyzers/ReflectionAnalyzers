namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL025ArgumentsDontMatchParameters.Descriptor;

            [Test]
            public void SingleIntParameter()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, new object[] { 1 });
        }

        public static int Bar(int value) => value;
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Invoke(null, null)")]
            [TestCase("Invoke(null, new object[0])")]
            [TestCase("Invoke(null, new object[0] { })")]
            [TestCase("Invoke(null, Array.Empty<object>())")]
            public void NoParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("Invoke(null, null)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
