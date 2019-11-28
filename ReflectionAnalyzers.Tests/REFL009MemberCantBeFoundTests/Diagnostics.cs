namespace ReflectionAnalyzers.Tests.REFL009MemberCantBeFoundTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL009MemberCannotBeFound);

        [TestCase("c.GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("new C().GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("this.GetType().GetMethod(↓\"MISSING\")")]
        [TestCase("GetType().GetMethod(↓\"MISSING\")")]
        public static void MissingMethod(string type)
        {
            var code = @"
namespace N
{
    public class C
    {
        public object M(C c) => typeof(C).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(C).GetMethod(↓\"MISSING\")", type);

            var message = "The referenced member MISSING is not known to exist in N.C.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public static void MissingPropertySetAccessor()
        {
            var code = @"
namespace N
{
    class C
    {
        public int P { get; }

        public static object Get(C c) => c.GetType().GetMethod(↓""set_P"");
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void MissingPropertyGetAccessor()
        {
            var code = @"
namespace N
{
    class C
    {
        public int P { set { } }

        public static object Get(C c) => c.GetType().GetMethod(↓""get_P"");
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void SubclassAggregateExceptionGetFieldDeclaredOnly()
        {
            var customAggregateException = @"
namespace N
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }
}";
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Get(CustomAggregateException c) => c.GetType()
                                                                     .GetField(↓""MISSING"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, customAggregateException, code);
        }

        [TestCase("GetProperty(↓\"Item\")")]
        [TestCase("GetProperty(↓\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void NamedIndexer(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        [IndexerName(""Bar"")]
        public int this[int i] => 0;

        public static object Get(C c) => c.GetType().GetProperty(""Item"");
    }
}".AssertReplace("GetProperty(\"Item\")", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
