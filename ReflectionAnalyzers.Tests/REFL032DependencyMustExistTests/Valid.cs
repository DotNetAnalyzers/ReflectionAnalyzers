namespace ReflectionAnalyzers.Tests.REFL032DependencyMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DependencyAttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL032DependencyMustExist;

        [Test]
        public static void GetMethodNoParameter()
        {
            var code = @"
using System.Runtime.CompilerServices;
[assembly: Dependency(""System.Collections"", LoadHint.Always)] ";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
