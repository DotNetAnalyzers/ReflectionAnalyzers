namespace ReflectionAnalyzers.Tests.REFL032DependencyMustExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DependencyAttributeAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL032DependencyMustExist);

        [Test]
        public static void GetMethodNoParameter()
        {
            var code = @"
using System.Runtime.CompilerServices;
[assembly: Dependency(↓""MISSING_DEPENDENCY"", LoadHint.Always)] ";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
