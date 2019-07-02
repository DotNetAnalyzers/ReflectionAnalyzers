namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MethodInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL030UseCorrectObj.Descriptor);

            [Test]
            public static void Static()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(C foo)
        {
            _ = typeof(C).GetMethod(nameof(Bar)).Invoke(↓foo, null);
        }

        public static void Bar()
        {
        }
    }
}";

                var message = "The method RoslynSandbox.C.Bar() is static and null should be passed as obj.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void Instance()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(Bar)).Invoke(↓null, null);
        }

        public int Bar() => 0;
    }
}";
                var message = "The method RoslynSandbox.C.Bar() is an instance method and the instance should be passed as obj.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void NullableInstance()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var value = typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(↓null, null);
        }
    }
}";
                var message = "The method int?.GetValueOrDefault() is an instance method and the instance should be passed as obj.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void InstanceWrongType()
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C(int i)
        {
            var value = (int)typeof(C).GetMethod(nameof(Bar)).Invoke(↓i, null);
        }

        public int Bar() => 0;
    }
}";
                var message = "Expected an argument of type RoslynSandbox.C.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
