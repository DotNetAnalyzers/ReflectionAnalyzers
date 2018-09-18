namespace ReflectionAnalyzers.Tests.REFL006RedundantBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL006");

        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Instance",              "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",             "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance",          "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic",         "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase",        "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static",              "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase",          "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static",              "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase",          "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static",                                          "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic",                                       "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy",                                "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly |  BindingFlags.Public",           "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly |  BindingFlags.Static",           "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly |  BindingFlags.FlattenHierarchy", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly |  BindingFlags.IgnoreCase",       "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethod(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), ↓BindingFlags.Public | BindingFlags.Static);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Static", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"The binding flags can be more precise. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.NonPublic",                                                    "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Static",                                                       "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Instance",                                                     "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.FlattenHierarchy",                                             "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",                         "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.NonPublic",                                                    "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Static",                                                       "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Instance",                                                     "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.FlattenHierarchy",                                             "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",                         "BindingFlags.Public | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Public",                                                     "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Static",                                                     "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Instance",                                                   "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.FlattenHierarchy",                                           "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly",                      "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Public",                                                     "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Static",                                                     "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Instance",                                                   "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.FlattenHierarchy",                                           "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        [TestCase("Private",       "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly",                      "BindingFlags.NonPublic | BindingFlags.DeclaredOnly")]
        public void GetNestedType(string type, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var typeInfo = typeof(Foo).GetNestedType(nameof(PublicStatic), ↓BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static class PublicStatic
        {
        }

        public class Public
        {
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})")
  .AssertReplace("BindingFlags.Public | BindingFlags.NonPublic", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var typeInfo = typeof(Foo).GetNestedType(nameof(PublicStatic), BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

        public static class PublicStatic
        {
        }

        public class Public
        {
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}".AssertReplace("nameof(PublicStatic)", $"nameof({type})")
  .AssertReplace("BindingFlags.Public | BindingFlags.DeclaredOnly", expected);
            var message = $"The binding flags can be more precise. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
