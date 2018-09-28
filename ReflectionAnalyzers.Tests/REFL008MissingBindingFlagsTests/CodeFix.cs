namespace ReflectionAnalyzers.Tests.REFL008MissingBindingFlagsTests
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
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL008");

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public void GetMethodNoFlags(string method, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public)↓);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetMethodNoParameterWithTypes()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), Type.EmptyTypes);
        }

        public int Bar() => 0;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        }

        public int Bar() => 0;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void GetMethodOneParameterWithTypes()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), new[] { typeof(int) });
        }

        public int Bar(int value) => value;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }

        public int Bar(int value) => value;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void GetMethodTwoParameterWithTYpes()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), new[] { typeof(int), typeof(double) });
        }

        public double Bar(int i, double d) => i + d;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(double) }, null);
        }

        public double Bar(int i, double d) => i + d;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void GetMethodNoFlagsFixInsideArrayInitializer()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            _ = new[]
            {
                GetType().GetMethod(nameof(ToString))
            };
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            _ = new[]
            {
                GetType().GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Instance)
            };
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic | BindingFlags.Static",   "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",  "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Public), ↓BindingFlags.Public);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public", flags);

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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        public void GetMethodWhenInvocationIsArgumentIssue64()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection.Emit;

    public class Foo
    {
        public Foo(ILGenerator il)
        {
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)↓));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public class Foo
    {
        public Foo(ILGenerator il)
        {
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("Type.EmptyTypes",             "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("Array.Empty<Type>()",         "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[0]",                 "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[1] { typeof(int) }", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[] { typeof(int) }",  "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new[] { typeof(int) }",       "BindingFlags.Public | BindingFlags.Instance")]
        public void GetConstructorWhenMissingFlags(string types, string flags)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            var ctor = typeof(Foo).GetConstructor↓(Type.EmptyTypes);
        }

        public Foo(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var ctor = typeof(Foo).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        public Foo(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types)
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance", flags);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("PublicStatic",    "BindingFlags.Public")]
        [TestCase("PublicInstance",  "BindingFlags.Public")]
        [TestCase("PrivateStatic",   "BindingFlags.NonPublic")]
        [TestCase("PrivateInstance", "BindingFlags.NonPublic")]
        public void GetNestedTypeNoFlags(string method, string expected)
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(nameof(PublicInstance)↓);
        }

        public static class PublicStatic
        {
        }

        public class PublicInstance
        {
        }

        private static class PrivateStatic
        {
        }

        private class PrivateInstance
        {
        }
    }
}".AssertReplace("nameof(PublicInstance)", $"nameof({method})");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(nameof(PublicInstance), BindingFlags.Public);
        }

        public static class PublicStatic
        {
        }

        public class PublicInstance
        {
        }

        private static class PrivateStatic
        {
        }

        private class PrivateInstance
        {
        }
    }
}".AssertReplace("nameof(PublicInstance)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public", expected);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
