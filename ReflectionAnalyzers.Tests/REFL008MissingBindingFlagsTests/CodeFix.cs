namespace ReflectionAnalyzers.Tests.REFL008MissingBindingFlagsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new BindingFlagsFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL008");

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public static void GetMethodNoFlags(string method, string expected)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public)↓);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})");

            var after = @"
namespace N
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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetMethodNoParameterWithTypes()
        {
            var before = @"
namespace N
{
    using System;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), Type.EmptyTypes↓);
        }

        public int M() => 0;
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
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        }

        public int M() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetMethodOneParameterWithTypes()
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), new[] { typeof(int) }↓);
        }

        public int M(int value) => value;
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
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }

        public int M(int value) => value;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetMethodTwoParameterWithTypes()
        {
            var before = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M), new[] { typeof(int), typeof(double) }↓);
        }

        public double M(int i, double d) => i + d;
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
            var methodInfo = typeof(C).GetMethod(nameof(this.M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(double) }, null);
        }

        public double M(int i, double d) => i + d;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetMethodNoFlagsFixInsideArrayInitializer()
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            _ = new[]
            {
                GetType().GetMethod(nameof(ToString)↓)
            };
        }
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
            _ = new[]
            {
                GetType().GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Instance)
            };
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("Static",        "BindingFlags.Public | BindingFlags.Static",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Public",   "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("this.ToString", "BindingFlags.Public | BindingFlags.Instance",    "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        [TestCase("PrivateStatic", "BindingFlags.NonPublic | BindingFlags.Static",   "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly")]
        [TestCase("this.Private",  "BindingFlags.NonPublic | BindingFlags.Instance", "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
        public static void GetMethod(string method, string flags, string expected)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public), ↓BindingFlags.Public);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public", flags);

            var after = @"
namespace N
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

        private static int PrivateStatic() => 0;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly", expected);

            var message = $"Specify binding flags for better performance and less fragile code. Expected: {expected}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void GetMethodWhenInvocationIsArgumentIssue64()
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection.Emit;

    public class C
    {
        public C(ILGenerator il)
        {
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)↓));
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public class C
    {
        public C(ILGenerator il)
        {
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("Type.EmptyTypes",             "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("Array.Empty<Type>()",         "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[0]",                 "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[1] { typeof(int) }", "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new Type[] { typeof(int) }",  "BindingFlags.Public | BindingFlags.Instance")]
        [TestCase("new[] { typeof(int) }",       "BindingFlags.Public | BindingFlags.Instance")]
        public static void GetConstructorWhenMissingFlags(string types, string flags)
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var ctor = typeof(C).GetConstructor↓(Type.EmptyTypes);
        }

        public C(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var ctor = typeof(C).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        public C(int value)
        {
        }
    }
}".AssertReplace("Type.EmptyTypes", types)
  .AssertReplace("BindingFlags.Public | BindingFlags.Instance", flags);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("PublicStatic",    "BindingFlags.Public")]
        [TestCase("PublicInstance",  "BindingFlags.Public")]
        [TestCase("PrivateStatic",   "BindingFlags.NonPublic")]
        [TestCase("PrivateInstance", "BindingFlags.NonPublic")]
        public static void GetNestedTypeNoFlags(string method, string expected)
        {
            var before = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetNestedType(nameof(PublicInstance)↓);
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

            var after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetNestedType(nameof(PublicInstance), BindingFlags.Public);
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
