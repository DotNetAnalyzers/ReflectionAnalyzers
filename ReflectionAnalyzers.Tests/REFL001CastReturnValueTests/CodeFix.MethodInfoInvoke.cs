namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class MethodInfoInvoke
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly CastReturnValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

        [TestCase("typeof(C).GetMethod(nameof(M)).Invoke(null, null)")]
        [TestCase("typeof(C).GetMethod(nameof(M))!.Invoke(null, null)")]
        [TestCase("typeof(C).GetMethod(nameof(M))?.Invoke(null, null)")]
        [TestCase("typeof(C).GetMethod(nameof(M))?.Invoke(null, null)!")]
        [TestCase("typeof(C).GetMethod(nameof(M))?.Invoke(null, null) ?? throw new Exception()")]
        public static void AssigningLocal(string expression)
        {
            var before = @"
#pragma warning disable CS8019, CS8602, CS8605
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var x = ↓typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(M)).Invoke(null, null)", expression);

            var after = @"
#pragma warning disable CS8019, CS8602, CS8605
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var x = (int)typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(M)).Invoke(null, null)", expression);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ReturningExpressionBody()
        {
            var before = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public object? Get() => ↓typeof(C).GetMethod(nameof(M)).Invoke(null, null);

        public static int M() => 0;
    }
}";

            var after = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public object? Get() => (int)typeof(C).GetMethod(nameof(M)).Invoke(null, null);

        public static int M() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ReturningStatementBody()
        {
            var before = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public object? Get()
        {
            return ↓typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}";

            var after = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public object? Get()
        {
            return (int)typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static int M() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
