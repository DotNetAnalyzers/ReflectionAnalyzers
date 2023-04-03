namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class ConstructorInfoInvoke
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly CastReturnValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

        [TestCase("typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })")]
        [TestCase("typeof(C).GetConstructor(new[] { typeof(int) })?.Invoke(new object[] { 1 })")]
        [TestCase("typeof(C).GetConstructor(new[] { typeof(int) })!.Invoke(new object[] { 1 })")]
        public static void AssigningLocal(string expression)
        {
            var before = @"
#pragma warning disable CS8600, CS8602
namespace N
{
    public class C
    {
        public C(int i)
        {
            var value = ↓typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}".AssertReplace("typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })", expression);

            var after = @"
#pragma warning disable CS8600, CS8602
namespace N
{
    public class C
    {
        public C(int i)
        {
            var value = (C)typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 });
        }
    }
}".AssertReplace("typeof(C).GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })", expression);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("info.Invoke(new object[] { 1 })")]
        [TestCase("info?.Invoke(new object[] { 1 })")]
        [TestCase("info!.Invoke(new object[] { 1 })")]
        public static void Walk(string expression)
        {
            var code = @"
#pragma warning disable CS8600, CS8602
namespace N
{
    public class C
    {
        public C(int i)
        {
            var info = typeof(C).GetConstructor(new[] { typeof(int) });
            var value = ↓info.Invoke(new object[] { 1 });
        }
    }
}".AssertReplace("info.Invoke(new object[] { 1 })", expression);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
