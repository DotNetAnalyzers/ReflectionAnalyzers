namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class ConstructorInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

            [TestCase("GetConstructor(Type.EmptyTypes).Invoke(null)")]
            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })")]
            public void InvokeWithOneArgument(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
        }

        public Foo(int value)
        {
        }

        public static Foo Create() => (Foo)typeof(Foo).GetConstructor(Type.EmptyTypes).Invoke(null);
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes).Invoke(null)", call);

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
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
            type.GetConstructor(Type.EmptyTypes).Invoke(instance, null);
        }
    }
}".AssertReplace("type.GetConstructor(Type.EmptyTypes).Invoke(instance, null)", call);

                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
