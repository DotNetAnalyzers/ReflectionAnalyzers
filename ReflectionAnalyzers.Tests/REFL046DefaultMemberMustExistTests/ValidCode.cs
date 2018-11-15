namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL046DefaultMemberMustExist.Descriptor;

        [Test]
        public void GetMethodNoParameter()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Exists"")]
public class Foo
{
    public int Value { get; set; }
}
";
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
