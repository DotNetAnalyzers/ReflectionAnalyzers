namespace ReflectionAnalyzers.Tests.REFL026MissingDefaultConstructorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL026NoDefaultConstructor.Descriptor);

        [TestCase("Activator.CreateInstance<↓C>()")]
        [TestCase("Activator.CreateInstance(typeof(↓C))")]
        public static void OneConstructorSingleIntParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(int i)
        {
            var foo = Activator.CreateInstance(typeof(↓C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(↓C))", call);

            var message = "No parameterless constructor defined for RoslynSandbox.C.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("Activator.CreateInstance<↓C>()")]
        [TestCase("Activator.CreateInstance(typeof(↓C))")]
        [TestCase("Activator.CreateInstance(typeof(↓C), false)")]
        public static void PrivateConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        private C()
        {
        }

        public object M() => Activator.CreateInstance<↓C>();
    }
}".AssertReplace("Activator.CreateInstance<↓C>()", call);

            var message = "No parameterless constructor defined for RoslynSandbox.C.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("Activator.CreateInstance<↓C>()")]
        [TestCase("Activator.CreateInstance(typeof(↓C))")]
        public static void OneConstructorSingleParams(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C(params int[] values)
        {
        }

        public object M() => Activator.CreateInstance<↓C>();
    }
}".AssertReplace("Activator.CreateInstance<↓C>()", call);

            var message = "No parameterless constructor defined for RoslynSandbox.C.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
