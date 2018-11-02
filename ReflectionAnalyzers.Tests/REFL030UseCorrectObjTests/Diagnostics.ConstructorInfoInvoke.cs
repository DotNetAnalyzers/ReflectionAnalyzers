namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        public class ConstructorInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })")]
            public void PassingNullAsObj(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(int value)
        {
            var foo = typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 });
        }
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) }).Invoke(null, new object[] { 1 })", call);

                var message = "Use overload of Invoke without obj parameter.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("GetConstructor(Type.EmptyTypes).Invoke(text, null)")]
            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(text, new object[] { 1 })")]
            public void InvokeWithGetUninitializedObjectAndArgument(string call)
            {
                var code = @"
namespace RoslynSandbox
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

        public static void Bar(string text)
        {
            typeof(C).GetConstructor(Type.EmptyTypes).Invoke(text, null);
        }
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes).Invoke(text, null)", call);

                var message = "Use an instance of type RoslynSandbox.C.";
                AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
