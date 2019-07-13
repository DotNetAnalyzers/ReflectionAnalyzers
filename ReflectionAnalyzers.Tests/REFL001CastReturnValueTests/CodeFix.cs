namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ActivatorCreateInstance
        {
            private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
            private static readonly CodeFixProvider Fix = new CastReturnValueFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

            [Test]
            public static void Typeof()
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }

            [Test]
            public static void WalkType()
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
            }

            [TestCase("Activator.CreateInstance(typeof(T))")]
            [TestCase("Activator.CreateInstance(typeof(T), true)")]
            [TestCase("Activator.CreateInstance(typeof(T), false)")]
            [TestCase("Activator.CreateInstance(typeof(T), \"foo\")")]
            public static void WhenUnconstrainedGeneric(string call)
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

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
