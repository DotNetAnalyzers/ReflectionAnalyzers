namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MethodInfoInvoke
        {
            private static readonly InvokeAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL030UseCorrectObj);

            [Test]
            public static void Static()
            {
                var code = @"
namespace N
{
    public class C
    {
        public C(C c)
        {
            _ = typeof(C).GetMethod(nameof(M)).Invoke(↓c, null);
        }

        public static void M()
        {
        }
    }
}";

                var message = "The method N.C.M() is static and null should be passed as obj.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void Instance()
            {
                var code = @"
namespace N
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(↓null, null);
        }

        public int M() => 0;
    }
}";
                var message = "The method N.C.M() is an instance method and the instance should be passed as obj.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void NullableInstance()
            {
                var code = @"
namespace N
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
namespace N
{
    public class C
    {
        public C(int i)
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(↓i, null);
        }

        public int M() => 0;
    }
}";
                var message = "Expected an argument of type N.C.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }
        }
    }
}
