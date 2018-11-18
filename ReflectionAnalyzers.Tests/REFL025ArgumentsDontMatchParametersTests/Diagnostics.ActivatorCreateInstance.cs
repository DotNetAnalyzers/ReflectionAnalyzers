namespace ReflectionAnalyzers.Tests.REFL025ArgumentsDontMatchParametersTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor);

            [TestCase("Activator.CreateInstance(typeof(C), ↓new[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { 1, 2 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓\"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1.0)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1, 2)")]
            public void OneConstructorSingleIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i)
        {
            var foo = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { 1, 2 })")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓\"abc\")")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1.0)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓1, 2)")]
            public void OneConstructorOptionalIntParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i = 0)
        {
            var foo = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓null)")]
            public void OneConstructorOneStringParameters(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(string text1)
        {
            var foo = Activator.CreateInstance(typeof(C), ↓null);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), ↓null)", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓(string)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓(StringBuilder)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { null })")]
            public void OverloadedConstructorsStringAndStringBuilder(string call)
            {
                var code = @"
namespace RoslynSandbox
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

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), ↓(string)null)")]
            [TestCase("Activator.CreateInstance(typeof(C), ↓new object[] { null })")]
            public void OverloadedConstructorsStringAndInt(string call)
            {
                var code = @"
namespace RoslynSandbox
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

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("Activator.CreateInstance(typeof(C), 1, null)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, new object[] { 1 })")]
            [TestCase("Activator.CreateInstance(typeof(C), \"abc\", 2)")]
            [TestCase("Activator.CreateInstance(typeof(C), 1, \"abc\")")]
            public void ParamsConstructorSecondParameter(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i, params int[] ints)
        {
            var foo = Activator.CreateInstance(typeof(C), 1, null);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), 1, null)", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
