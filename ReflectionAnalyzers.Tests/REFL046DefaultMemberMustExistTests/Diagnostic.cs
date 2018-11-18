namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostic
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL046DefaultMemberMustExist.Descriptor);

        /// <summary>
        /// Verify diagnostic is present when no such member exists.
        /// </summary>
        [Test]
        public void DefaultMemberAbsent()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""NotValue"")]
public class Foo
{
    public int Value { get; set; }
}
";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        /// <summary>
        /// Verify events are not considered valid targets.
        /// </summary>
        [Test]
        public void DefaultMemberIsEvent()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Event"")]
public class Foo
{
    public event EventhHandler Event;
}
";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
