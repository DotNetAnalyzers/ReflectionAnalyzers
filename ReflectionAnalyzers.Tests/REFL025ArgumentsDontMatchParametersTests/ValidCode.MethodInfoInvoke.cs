namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL025ArgumentsDontMatchParameters.Descriptor;

            [Test]
            public static void SingleIntParameter()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { 1 });
        }

        public static int M(int value) => value;
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Invoke(null, null)")]
            [TestCase("Invoke(null, new object[0])")]
            [TestCase("Invoke(null, new object[0] { })")]
            [TestCase("Invoke(null, Array.Empty<object>())")]
            public static void NoParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static object Get => typeof(C).GetMethod(nameof(M)).Invoke(null, null);

        public static int M() => 1;
    }
}".AssertReplace("Invoke(null, null)", call);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("1")]
            [TestCase("Missing.Value")]
            public static void OptionalParameter(string value)
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public static int Get => (int)typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { Missing.Value });

        public static int M(int value = 1) => value;
    }
}".AssertReplace("Missing.Value", value);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
