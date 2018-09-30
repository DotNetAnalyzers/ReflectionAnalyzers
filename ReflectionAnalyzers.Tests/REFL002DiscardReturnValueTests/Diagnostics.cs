namespace ReflectionAnalyzers.Tests.REFL002DiscardReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL002DiscardReturnValue.Descriptor);

        [Test]
        public void AssigningLocal()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = ↓typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void AssigningField()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly int value;

        public Foo()
        {
            this.value = ↓typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void UsingInExpression()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var text = ((int)↓typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null)).ToString();
        }

        public static void Bar()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("type.GetConstructor(Type.EmptyTypes).Invoke(instance, null)")]
        [TestCase("type.GetConstructor(new[] { typeof(int) }).Invoke(instance, new object[] { 1 })")]
        public void InvokeWithGetUninitializedObjectAndArgument(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Runtime.Serialization;

    public class Foo
    {
        public Foo()
        {
        }

        public Foo(int value)
        {
        }

        public static void Bar()
        {
            var type = typeof(Foo);
            var instance = FormatterServices.GetUninitializedObject(type);
            var foo = type.GetConstructor(Type.EmptyTypes).Invoke(instance, null);
        }
    }
}".AssertReplace("type.GetConstructor(Type.EmptyTypes).Invoke(instance, null)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
