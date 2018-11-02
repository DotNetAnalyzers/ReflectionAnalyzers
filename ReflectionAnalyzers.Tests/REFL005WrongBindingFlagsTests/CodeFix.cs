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
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL005WrongBindingFlags.Descriptor);

        [TestCase("Static", "BindingFlags.Public | BindingFlags.Instance", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("ReferenceEquals", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Public", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.GetHashCode", "BindingFlags.Public", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("this.Private", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private", "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
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
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetMethodWithTrivia()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(
                nameof(this.Public),
  /* trivia1 */ ↓BindingFlags.Public | BindingFlags.Static    /* trivia2 */  );
        }

        public int Public() => 0;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(
                nameof(this.Public),
  /* trivia1 */ BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly    /* trivia2 */  );
        }

        public int Public() => 0;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("this.ToString", "BindingFlags.Public", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodWhenShadowed(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString), ↓BindingFlags.Static);
        }

        public new string ToString() => string.Empty;
    }
}".AssertReplace("nameof(this.ToString)", $"nameof({method})")
  .AssertReplace("BindingFlags.Static", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public new string ToString() => string.Empty;
    }
}".AssertReplace("nameof(this.ToString)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("ReferenceEquals", "BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy")]
        [TestCase("this.Private", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodWhenMissingFlags(string method, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public));
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

            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("StaticMethod", "Public | Instance", "Public | Static | DeclaredOnly")]
        [TestCase("StaticMethod", "Public | Instance | DeclaredOnly", "Public | Static | DeclaredOnly")]
        [TestCase("StaticMethod", "NonPublic | Static", "Public | Static | DeclaredOnly")]
        [TestCase("ReferenceEquals", "Public | Static", "Public | Static | FlattenHierarchy")]
        [TestCase("this.PublicMethod", "Public | Static", "Public | Instance | DeclaredOnly")]
        [TestCase("this.PublicMethod", "NonPublic | Instance", "Public | Instance | DeclaredOnly")]
        [TestCase("this.PublicMethod", "NonPublic | Instance | DeclaredOnly", "Public | Instance | DeclaredOnly")]
        [TestCase("this.PublicMethod", "Public | Static | DeclaredOnly", "Public | Instance | DeclaredOnly")]
        [TestCase("this.ToString", "Public", "Public | Instance | DeclaredOnly")]
        [TestCase("this.ToString", "NonPublic | Static", "Public | Instance | DeclaredOnly")]
        [TestCase("this.ToString", "Public | Static", "Public | Instance | DeclaredOnly")]
        [TestCase("this.GetHashCode", "Public", "Public | Instance")]
        [TestCase("this.GetHashCode", "Public | Instance | DeclaredOnly", "Public | Instance")]
        [TestCase("this.GetHashCode", "NonPublic | Static", "Public | Instance")]
        [TestCase("this.GetHashCode", "Public | Static", "Public | Instance")]
        [TestCase("this.PrivateMethod", "Public | Instance | DeclaredOnly", "NonPublic | Instance | DeclaredOnly")]
        [TestCase("this.PrivateMethod", "NonPublic | Static | DeclaredOnly", "NonPublic | Instance | DeclaredOnly")]
        public void GetMethodUsingStatic(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using static System.Reflection.BindingFlags;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.PublicMethod), ↓Public | Static);
        }

        public static int StaticMethod() => 0;

        public int PublicMethod() => 0;

        public override string ToString() => string.Empty;

        private int PrivateMethod() => 0;
    }
}".AssertReplace("nameof(this.PublicMethod)", $"nameof({method})")
  .AssertReplace("Public | Static", flags);
            var fixedCode = @"
namespace RoslynSandbox
{
    using static System.Reflection.BindingFlags;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.PublicMethod), Public | Instance | DeclaredOnly);
        }

        public static int StaticMethod() => 0;

        public int PublicMethod() => 0;

        public override string ToString() => string.Empty;

        private int PrivateMethod() => 0;
    }
}".AssertReplace("nameof(this.PublicMethod)", $"nameof({method})")
  .AssertReplace("Public | Instance | DeclaredOnly", expected);
            var message = $"There is no member matching the filter. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("Static", "BindingFlags.Public | BindingFlags.Instance", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("Static", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.Public | BindingFlags.Static", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public", "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private", "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetProperty(string method, string flags, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(this.Public), ↓BindingFlags.Public | BindingFlags.Static);
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

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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

        [TestCase("PublicStatic", "BindingFlags.NonPublic", "BindingFlags.Public")]
        [TestCase("PublicStatic", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public")]
        [TestCase("PublicStatic", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public")]
        [TestCase("Public", "BindingFlags.NonPublic | BindingFlags.Static", "BindingFlags.Public")]
        [TestCase("Public", "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.Public")]
        [TestCase("Public", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.Public")]
        [TestCase("PrivateStatic", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic")]
        [TestCase("Private", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", "BindingFlags.NonPublic")]
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
            var typeInfo = typeof(C).GetNestedType(nameof(PublicStatic), ↓BindingFlags.Public | BindingFlags.Static);
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

    class C
    {
        public C()
        {
            var typeInfo = typeof(C).GetNestedType(nameof(PrivateStatic));
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

    class C
    {
        public C()
        {
            var typeInfo = typeof(C).GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic);
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

        [TestCase("Type.EmptyTypes")]
        [TestCase("Array.Empty<Type>()")]
        [TestCase("new Type[0]")]
        [TestCase("new Type[1] { typeof(double) }")]
        [TestCase("new Type[] { typeof(double) }")]
        [TestCase("new[] { typeof(double) }")]
        public void GetConstructorWhenMissingFlags(string types)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        private C()
        {
            var ctor = typeof(C).GetConstructor↓(Type.EmptyTypes);
        }

        private C(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        private C()
        {
            var ctor = typeof(C).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        private C(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
