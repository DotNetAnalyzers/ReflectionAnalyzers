namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class ConstructorInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL030UseCorrectObj);

            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })")]
            public static void PassingNullAsObj(string call)
            {
                var code = @"
namespace N
{
    public class C
    {
        public C(int value)
        {
        }

        public object Get() => typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 });
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })", call);

                var message = "Use overload of Invoke without obj parameter.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("GetConstructor(Type.EmptyTypes).Invoke(text, null)")]
            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(text, new object[] { 1 })")]
            public static void InvokeWithGetUninitializedObjectAndArgument(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
        }

        public C(int value)
        {
        }

        public static object Get(String text) => typeof(C).GetConstructor(Type.EmptyTypes).Invoke(text, null);
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes).Invoke(text, null)", call);

                var message = "Use an instance of type N.C.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
