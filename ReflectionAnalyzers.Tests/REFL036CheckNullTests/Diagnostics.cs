namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly GetTypeAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL036CheckNull);

        [TestCase("Get() => System.Type.GetType(\"C\").Assembly")]
        [TestCase("Get() => System.Type.GetType(\"C\", throwOnError: false).Assembly")]
        [TestCase("Get(System.Reflection.Assembly source) => source.GetType(\"C\").Assembly")]
        [TestCase("Get(System.Reflection.Assembly source) => source.GetType(\"C\", throwOnError: false).Assembly")]
        [TestCase("Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\").Assembly")]
        [TestCase("Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\", throwOnError: false).Assembly")]
        public static void WhenMemberAccess(string body)
        {
            var code = @"
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public static object? Get() => Type.GetType(""C"").Assembly;
    }
}".AssertReplace("Get() => Type.GetType(\"C\").Assembly", body);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
