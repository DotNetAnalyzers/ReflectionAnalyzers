namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor);

            [TestCase("Activator.CreateInstance(typeof(C), ↓new[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { 1, 2 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓\"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1.0)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1, 2)")]
            public static void OneConstructorSingleIntParameter(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i)
        {
            var c = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { 1, 2 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓\"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1.0)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1, 2)")]
            public static void OneConstructorOptionalIntParameter(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i = 0)
        {
            var c = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓null)")]
            public static void OneConstructorOneStringParameters(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(string text1)
        {
            var c = Activator.CreateInstance(typeof(C), ↓null);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), ↓null)", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓(string)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓(StringBuilder)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { null })")]
            public static void OverloadedConstructorsStringAndStringBuilder(string call)
            {
                var code = @"
namespace N
{
    using System;
    using System.Text;

    public class C
    {
        public C(string text)
        {
        }

        public C(StringBuilder text)
        {
        }

        public static C Create() => (C)Activator.CreateInstance(typeof(C), ↓(string)null);
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), ↓(string)null)", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓(string)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { null })")]
            public static void OverloadedConstructorsStringAndInt(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(string text)
        {
        }

        public C(int value)
        {
        }

        public static C Create() => (C)Activator.CreateInstance(typeof(C), ↓(string)null);
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), ↓(string)null)", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), 1, null)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, new object[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), \"abc\", 2)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, \"abc\")")]
            public static void ParamsConstructorSecondParameter(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i, params int[] ints)
        {
            var c = Activator.CreateInstance(typeof(C), 1, null);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1, null)", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
