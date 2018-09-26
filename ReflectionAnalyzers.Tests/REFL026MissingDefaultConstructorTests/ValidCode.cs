namespace ReflectionAnalyzers.Tests.REFL026MissingDefaultConstructorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL026MissingDefaultConstructor.Descriptor);

        [TestCase("Activator.CreateInstance(typeof(Foo))")]
        [TestCase("Activator.CreateInstance(typeof(Foo), true)")]
        [TestCase("Activator.CreateInstance(typeof(Foo), false)")]
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
        public void OneConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(int i)
        {
            var foo = Activator.CreateInstance(typeof(Foo));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(Foo))", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void PrivateConstructor()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private Foo()
        {
            var foo = Activator.CreateInstance(typeof(Foo), true);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
