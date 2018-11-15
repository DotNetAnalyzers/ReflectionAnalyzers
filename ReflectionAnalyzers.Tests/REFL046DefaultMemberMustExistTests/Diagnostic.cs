namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostic
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL046DefaultMemberMustExist.Descriptor);

        [Test]
        public void DefaultMemberAbsent()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Bar"")]
public class Foo
{
    public int Value { get; set; }
}
";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }


    }
}
