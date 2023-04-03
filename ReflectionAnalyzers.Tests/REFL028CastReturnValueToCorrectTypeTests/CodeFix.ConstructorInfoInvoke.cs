namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class ConstructorInfoInvoke
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly CastReturnValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL028CastReturnValueToCorrectType);

        [Test]
        public static void WhenCastingToWrongType()
        {
            var before = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public class C
    {
        public C(int i)
        {
            var value = (↓string)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";

            var after = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public class C
    {
        public C(int i)
        {
            var value = (C)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
