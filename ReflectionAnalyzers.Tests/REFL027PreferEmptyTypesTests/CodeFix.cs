namespace ReflectionAnalyzers.Tests.REFL027PreferEmptyTypesTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly ArgumentAnalyzer Analyzer = new();
    private static readonly PreferEmptyTypesFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL027PreferEmptyTypes);

    [TestCase("new Type[0]")]
    [TestCase("Array.Empty<Type>()")]
    [TestCase("new Type[0] { }")]
    [TestCase("new Type[] { }")]
    public static void GetConstructor(string emptyArray)
    {
        var before = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            _ = typeof(C).GetConstructor(new Type[0]);
        }
    }
}".AssertReplace("new Type[0]", emptyArray);

        var after = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            _ = typeof(C).GetConstructor(Type.EmptyTypes);
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
