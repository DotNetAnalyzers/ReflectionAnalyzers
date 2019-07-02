namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor);

            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, new object[] { ↓1.2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, new object[] { ↓\"abc\" })")]
            public static void SingleIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 });
        }

        public static int Bar(int value) => value;
    }
}".AssertReplace("GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 })", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1.2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.Bar)).Invoke(null, ↓new object[] { \"abc\" })")]
            public static void NoParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            typeof(C).GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 });
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(Bar)).Invoke(null, new object[] { ↓1.2 })", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void ObjectParameterMissingValue()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public static object Get => typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { ↓Missing.Value });

        public static int M(object value) => 0;
    }
}";

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("new object[0]")]
            [TestCase("null")]
            public static void OptionalParameterMissingValue(string args)
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public static object Get => typeof(C).GetMethod(nameof(M)).Invoke(null, new object[0]);

        public static int M(int value = 0) => value;
    }
}".AssertReplace("new object[0]", args);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
