namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        public static class GetMethod
        {
            private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
            private static readonly CodeFixProvider Fix = new DisambiguateFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL004AmbiguousMatch);

            [Test]
            public static void PublicOverloads()
            {
                var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod↓(nameof(Static));
        }

        public static double Static(int value) => value;

        public static double Static(double value) => value;
    }
}";

                var after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }

        public static double Static(int value) => value;

        public static double Static(double value) => value;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Use: new[] { typeof(int) }.");

                after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null);
        }

        public static double Static(int value) => value;

        public static double Static(double value) => value;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Use: new[] { typeof(double) }.");
            }

            [Test]
            public static void TwoIndexers()
            {
                var before = @"
namespace N
{
    public class C
    {
        public object Get => typeof(C).GetProperty↓(""Item"");

        public int this[int i] => 0;

        public int this[int i1, int i2] => 0;
    }
}";

                var after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(C).GetProperty(""Item"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null);

        public int this[int i] => 0;

        public int this[int i1, int i2] => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Use: new[] { typeof(int) }.");

                after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(C).GetProperty(""Item"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int), typeof(int) }, null);

        public int this[int i] => 0;

        public int this[int i1, int i2] => 0;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Use: new[] { typeof(int), typeof(int) }.");
            }
        }
    }
}
