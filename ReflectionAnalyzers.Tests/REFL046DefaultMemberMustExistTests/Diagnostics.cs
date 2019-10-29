namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL046DefaultMemberMustExist.Descriptor);

        /// <summary>
        /// Verify diagnostic is present when no such member exists.
        /// </summary>
        [Test]
        public static void DefaultMemberAbsent()
        {
            var code = @"
using System.Reflection;
[DefaultMember(↓""NotValue"")]
public class Foo
{
    public int Value { get; set; }
}
";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        /// <summary>
        /// Verify events are not considered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberIsEvent()
        {
            var code = @"
using System;
using System.Reflection;

[DefaultMember(↓""Event"")]
public class Foo
{
    public event EventHandler Event;
}
";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        /// <summary>
        /// Verify base class names are not considered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberIsBaseClass()
        {
            var code = @"
using System.Reflection;

public class Base 
{
    Base() 
    {
        System.Console.WriteLine(""Base constructor"");
    }
}

[DefaultMember(↓""Base"")]
public class Foo { }
";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
