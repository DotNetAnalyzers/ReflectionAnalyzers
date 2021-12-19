namespace ReflectionAnalyzers.Tests.REFL036CheckNullTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly GetTypeAnalyzer Analyzer = new();
        private static readonly ThrowOnErrorFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL036CheckNull);

        [TestCase("Get() => Type.GetType(\"C\").Assembly",                                                                     "Get() => Type.GetType(\"C\", throwOnError: true).Assembly")]
        [TestCase("Get() => Type.GetType(\"C\", throwOnError: false).Assembly",                                                "Get() => Type.GetType(\"C\", throwOnError: true).Assembly")]
        [TestCase("Get(System.Reflection.Assembly source) => source.GetType(\"C\").Assembly",                                  "Get(System.Reflection.Assembly source) => source.GetType(\"C\", throwOnError: true).Assembly")]
        [TestCase("Get(System.Reflection.Assembly source) => source.GetType(\"C\", throwOnError: false).Assembly",             "Get(System.Reflection.Assembly source) => source.GetType(\"C\", throwOnError: true).Assembly")]
        [TestCase("Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\").Assembly",                      "Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\", throwOnError: true).Assembly")]
        [TestCase("Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\", throwOnError: false).Assembly", "Get(System.Reflection.Emit.AssemblyBuilder source) => source.GetType(\"C\", throwOnError: true).Assembly")]
        public static void WhenMemberAccess(string beforeExpression, string afterExpression)
        {
            var before = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public class C
    {
        public static object Get() => Type.GetType(""C"").Assembly;
    }
}".AssertReplace("Get() => Type.GetType(\"C\").Assembly", beforeExpression);

            var after = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public class C
    {
        public static object Get() => Type.GetType(""C"", throwOnError: true).Assembly;
    }
}".AssertReplace("Get() => Type.GetType(\"C\", throwOnError: true).Assembly", afterExpression);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
