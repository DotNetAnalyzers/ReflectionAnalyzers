namespace ReflectionAnalyzers.Tests.REFL025ActivatorCreateInstanceArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL025ActivatorCreateInstanceArguments.Descriptor);

        [TestCase("Activator.CreateInstance(typeof(Foo))")]
        [TestCase("Activator.CreateInstance(this.GetType())")]
        [TestCase("Activator.CreateInstance<Foo>()")]
        public void ExplicitDefaultConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            var foo = Activator.CreateInstance(typeof(Foo));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo))", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("(Foo)Activator.CreateInstance(typeof(Foo))")]
        [TestCase("Activator.CreateInstance<Foo>()")]
        public void ImplicitDefaultConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static Foo Create() => Activator.CreateInstance<Foo>();
    }
}".AssertReplace("Activator.CreateInstance<Foo>()", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("Activator.CreateInstance(typeof(Foo), 1)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { 1 })")]
        public void OneConstructorOneIntParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(int i)
        {
            var foo = Activator.CreateInstance(typeof(Foo), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo), 1)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("Activator.CreateInstance(typeof(Foo), \"abc\")")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { null })")]
        [TestCase("Activator.CreateInstance(typeof(Foo), (string)null)")]
        public void OneConstructorSingleStringParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(string text)
        {
            var foo = Activator.CreateInstance(typeof(Foo), ""abc"");
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo), \"abc\")", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("Activator.CreateInstance(typeof(Foo), 1)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), 1.2)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { 1 })")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { 1.2 })")]
        public void OneConstructorOneDoubleParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(double d)
        {
            var foo = Activator.CreateInstance(typeof(Foo), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo), 1)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("Activator.CreateInstance(typeof(Foo))")]
        [TestCase("Activator.CreateInstance(typeof(Foo), 1)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), 1, 2)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { 1 })")]
        [TestCase("Activator.CreateInstance(typeof(Foo), new object[] { 1, 2 })")]
        public void ParamsConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(params int[] ints)
        {
            var foo = Activator.CreateInstance(typeof(Foo), 1);
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo), 1)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
