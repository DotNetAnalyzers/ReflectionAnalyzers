namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL030UseCorrectObj.Descriptor;

            [Test]
            public void Static()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(new Foo(), null);
        }

        public int Bar() => 0;
    }
}";

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null)")]
            [TestCase("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke((int?)42, null)")]
            public void Invoke(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public static class Foo
    {
        public static object Bar() => typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null);
    }
}".AssertReplace("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null)", call);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
