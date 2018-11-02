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
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL006RedundantBindingFlags.Descriptor);

        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Instance",               "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",              "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",       "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance",           "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic",          "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase",         "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static",               "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",            "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",     "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static",               "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy",     "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic",            "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static",                                           "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic",                                        "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy",                                 "BindingFlags.Public | BindingFlags.Instance")]
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

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public), ↓BindingFlags.Public | BindingFlags.Static);
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

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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

        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.NonPublic",                                                    "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Static",                                                       "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Instance",                                                     "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.FlattenHierarchy",                                             "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.DeclaredOnly",                                                 "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",                         "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.NonPublic",                                                    "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Static",                                                       "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Instance",                                                     "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.DeclaredOnly",                                                 "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",                         "BindingFlags.Public")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Public",                                                     "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Static",                                                     "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Instance",                                                   "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.FlattenHierarchy",                                           "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.DeclaredOnly",                                               "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly",                      "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Public",                                                     "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Static",                                                     "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Instance",                                                   "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.FlattenHierarchy",                                           "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly",                      "BindingFlags.NonPublic")]
        public void GetNestedType(string type, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var typeInfo = typeof(C).GetNestedType(nameof(PublicStatic), ↓BindingFlags.Public | BindingFlags.NonPublic);
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

    class C
    {
        public C()
        {
            var typeInfo = typeof(C).GetNestedType(nameof(PublicStatic), BindingFlags.Public | BindingFlags.DeclaredOnly);
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

        [TestCase("GetConstructor(↓BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)")]
        [TestCase("GetConstructor(↓BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null)")]
        public void GetConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(C).GetConstructor(↓BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        }
    }
}".AssertReplace("GetConstructor(↓BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)", call);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(C).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }
    }
}";
            var message = "The binding flags can be more precise. Expected: BindingFlags.Public | BindingFlags.Instance.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
