namespace ReflectionAnalyzers.Tests.REFL017DontUseNameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    public partial class CodeFix
    {
        public class WhenInAccessibleMember
        {
            private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
            private static readonly CodeFixProvider Fix = new NameofFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL017DontUseNameofWrongMember.Descriptor);

            [Test]
            public void WrongContainingTypeWhenNotAccessible()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetProperty(↓nameof(this.InnerExceptionCount), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";
                var message = "Don't use name of wrong member. Expected: \"InnerExceptionCount\"";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), testCode, fixedCode, fixTitle: "Use \"InnerExceptionCount\".");
            }

            [Test]
            public void NonPublicNotVisible()
            {
                var exception = @"
namespace RoslynSandbox
{
    using System;
     public class CustomAggregateException : AggregateException
    {
        public int InnerExceptionCount { get; }
    }
}";
                var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;
     class Foo
    {
        public Foo()
        {
            var member = typeof(CustomAggregateException).GetProperty(↓nameof(CustomAggregateException.InnerExceptionCount), BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
                var fixedCode = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;
     class Foo
    {
        public Foo()
        {
            var member = typeof(CustomAggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { exception, code }, fixedCode);
            }
        }
    }
}
