namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

        [Test]
        public void Static()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(Foo foo)
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(↓foo, null);
        }

        public static void Bar()
        {
        }
    }
}";

            var message = "The method RoslynSandbox.Foo.Bar() is static and null should be passed as obj.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void Instance()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(↓null, null);
        }

        public int Bar() => 0;
    }
}";
            var message = "The method RoslynSandbox.Foo.Bar() is an instance method and the instance should be passed as obj.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void NullableInstance()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            var value = typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(↓null, null);
        }
    }
}";
            var message = "The method int?.GetValueOrDefault() is an instance method and the instance should be passed as obj.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public void InstanceWrongType()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int i)
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(↓i, null);
        }

        public int Bar() => 0;
    }
}";
            var message = "Expected an argument of type RoslynSandbox.Foo.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }
    }
}
