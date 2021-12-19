namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDoNotMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MethodInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL025ArgumentsDoNotMatchParameters);

            [TestCase("GetMethod(nameof(this.M)).Invoke(null, new object[] { ↓1.2 })")]
            [TestCase("GetMethod(nameof(this.M)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.M)).Invoke(null, new object[] { ↓\"abc\" })")]
            public static void SingleIntParameter(string call)
            {
                var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { ↓1.2 });
        }

        public static int M(int value) => value;
    }
}".AssertReplace("GetMethod(nameof(M)).Invoke(null, new object[] { ↓1.2 })", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("GetMethod(nameof(this.M)).Invoke(null, ↓new object[] { 1.2 })")]
            [TestCase("GetMethod(nameof(this.M)).Invoke(null, ↓new object[] { 1, 2 })")]
            [TestCase("GetMethod(nameof(this.M)).Invoke(null, ↓new object[] { \"abc\" })")]
            public static void NoParameter(string call)
            {
                var code = @"
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public C()
        {
            typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { ↓1.2 });
        }

        public static void M()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(M)).Invoke(null, new object[] { ↓1.2 })", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void ObjectParameterMissingValue()
            {
                var code = @"
#pragma warning disable CS8602
namespace N
{
    using System.Reflection;

    public class C
    {
        public static object? Get => typeof(C).GetMethod(nameof(M)).Invoke(null, new object[] { ↓Missing.Value });

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
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public static object? Get => typeof(C).GetMethod(nameof(M)).Invoke(null, new object[0]);

        public static int M(int value = 0) => value;
    }
}".AssertReplace("new object[0]", args);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
