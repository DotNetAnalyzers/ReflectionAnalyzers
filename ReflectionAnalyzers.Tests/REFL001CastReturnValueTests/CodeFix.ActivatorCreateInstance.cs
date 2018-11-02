namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public partial class CodeFix
    {
        public class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

            [Test]
            public void Typeof()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar()
        {
            var foo = ↓Activator.CreateInstance(typeof(C));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar()
        {
            var foo = (C)Activator.CreateInstance(typeof(C));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }

            [Test]
            public void WalkType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar()
        {
            var type = typeof(C);
            var foo = ↓Activator.CreateInstance(type);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar()
        {
            var type = typeof(C);
            var foo = (C)Activator.CreateInstance(type);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }

            [TestCase("Activator.CreateInstance(typeof(T))")]
            [TestCase("Activator.CreateInstance(typeof(T), true)")]
            [TestCase("Activator.CreateInstance(typeof(T), false)")]
            [TestCase("Activator.CreateInstance(typeof(T), \"foo\")")]
            public void WhenUnconstrainedGeneric(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar<T>()
        {
            var foo = ↓Activator.CreateInstance(typeof(T));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(T))", call);

                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
