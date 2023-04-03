namespace ReflectionAnalyzers.Tests.REFL037TypeDoesNotExitsTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class NoFix
{
    private static readonly GetTypeAnalyzer Analyzer = new();
    private static readonly SuggestTypeFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL037TypeDoesNotExits);

    [TestCase("MISSING")]
    //// [TestCase("N.MISSING")]
    public static void TypeGetTypeNoFix(string type)
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? Get => Type.GetType(↓""MISSING"");
    }
}".AssertReplace("MISSING", type);

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }

    [TestCase("typeof(C).Assembly.GetType(↓\"MISSING\")")]
    [TestCase("typeof(C).Assembly.GetType(↓\"N.MISSING\")")]
    public static void AssemblyGetTypeNoFix(string call)
    {
        var code = @"
namespace N
{
    public class C
    {
        public static object? Get => typeof(C).Assembly.GetType(↓""MISSING"");
    }
}".AssertReplace("typeof(C).Assembly.GetType(↓\"MISSING\")", call);

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }
}
