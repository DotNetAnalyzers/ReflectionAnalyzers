namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class DelegateCreateDelegate
    {
        private static readonly CreateDelegateAnalyzer Analyzer = new();
        private static readonly CastReturnValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

        [TestCase("typeof(C).GetMethod(nameof(M))")]
        [TestCase("typeof(C).GetMethod(nameof(M))!")]
        public static void AssigningLocal(string expression)
        {
            var before = @"
#pragma warning disable CS8604
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var value = ↓Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(M))", expression);

            var after = @"
#pragma warning disable CS8604
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                typeof(C).GetMethod(nameof(M)));
        }

        public static int M() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(M))", expression);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void Walk()
        {
            var before = @"
#pragma warning disable CS8604
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = ↓Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
        }

        public static int M() => 0;
    }
}";

            var after = @"
#pragma warning disable CS8604
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var info = typeof(C).GetMethod(nameof(M));
            var value = (Func<int>)Delegate.CreateDelegate(
                typeof(Func<int>),
                info);
        }

        public static int M() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
