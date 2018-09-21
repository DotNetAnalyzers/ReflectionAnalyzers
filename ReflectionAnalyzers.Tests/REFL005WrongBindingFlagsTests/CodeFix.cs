namespace ReflectionAnalyzers.Tests.REFL005WrongBindingFlagsTests
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
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL005");

        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance",                                "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance",                             "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public",                                                        "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.GetHashCode", "BindingFlags.Public",                                                        "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.Private",     "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly",   "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
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
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("this.ToString", "BindingFlags.Public",                          "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Static",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodWhenShadowed(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), ↓BindingFlags.Static);
        }

        public new string ToString() => string.Empty;
    }
}".AssertReplace("nameof(this.ToString)", $"nameof({method})")
  .AssertReplace("BindingFlags.Static", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public new string ToString() => string.Empty;
    }
}".AssertReplace("nameof(this.ToString)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("ReferenceEquals", "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Private",    "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodWhenMissingFlags(string method, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public));
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})");
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
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance",                                "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static",           "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static",                                  "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance",                             "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly",      "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly",   "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetProperty(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetProperty(nameof(this.Public), ↓BindingFlags.Public | BindingFlags.Static);
        }

        public static int Static => 0;

        public int Public => 0;

        private static int PrivateStatic => 0;

        private int Private => 0;
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
            var methodInfo = typeof(Foo).GetProperty(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int Static => 0;

        public int Public => 0;

        private static int PrivateStatic => 0;

        private int Private => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("PublicStatic",  "BindingFlags.NonPublic",                                                     "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public")]
        [TestCase("PublicStatic",  "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.NonPublic | BindingFlags.Static",                               "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.NonPublic | BindingFlags.Instance",                             "BindingFlags.Public")]
        [TestCase("Public",        "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public")]
        [TestCase("PrivateStatic", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.NonPublic")]
        [TestCase("Private",       "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly",    "BindingFlags.NonPublic")]
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
            var typeInfo = typeof(Foo).GetNestedType(nameof(PublicStatic), ↓BindingFlags.Public | BindingFlags.Static);
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
  .AssertReplace("BindingFlags.Public | BindingFlags.Static", flags);
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
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("PrivateStatic")]
        [TestCase("Private")]
        public void GetNestedTypeWhenMissingFlags(string type)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var typeInfo = typeof(Foo).GetNestedType(nameof(PrivateStatic));
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}".AssertReplace("nameof(PrivateStatic)", $"nameof({type})");
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var typeInfo = typeof(Foo).GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic);
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}".AssertReplace("nameof(PrivateStatic)", $"nameof({type})");
            var message = "There is no member matching the filter. Expected: BindingFlags.NonPublic.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
