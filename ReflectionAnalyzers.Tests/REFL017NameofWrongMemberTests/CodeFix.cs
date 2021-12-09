namespace ReflectionAnalyzers.Tests.REFL017NameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WhenInAccessibleMember
        {
            private static readonly GetXAnalyzer Analyzer = new();
            private static readonly NameofFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL017NameofWrongMember);

            [Test]
            public static void WrongContainingTypeWhenNotAccessible()
            {
                var before = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(↓nameof(this.InnerExceptionCount), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";

                var after = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";
                var message = "Don't use name of wrong member. Expected: \"InnerExceptionCount\"";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after, fixTitle: "Use \"InnerExceptionCount\".");
            }

            [Test]
            public static void NonPublicNotVisible()
            {
                var customAggregateException = @"
namespace N
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        public int InnerExceptionCount { get; }
    }
}";
                var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(CustomAggregateException).GetProperty(↓nameof(CustomAggregateException.InnerExceptionCount), BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
                var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(CustomAggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { customAggregateException, before }, after);
            }
        }
    }
}
