namespace ReflectionAnalyzers.Tests.REFL026MissingDefaultConstructorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ActivatorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL026NoDefaultConstructor.Descriptor);

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
        public void OneConstructorSingleIntParameter(string call)
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


        [Test]
        public void WhenUnknown()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static object Bar(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void WhenUnconstrainedGeneric()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static object Bar<T>() => Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
