namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class GetMethod
        {
            [Test]
            public static void Message()
            {
                var code = @"
namespace N
{
    public struct C
    {
        public C(int _)
        {
            var methodInfo = typeof(C).GetMethod(↓""MISSING"");
        }
    }
}";
                var message = "The type N.C does not have a member named MISSING";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("typeof(C).GetMethod(↓\"MISSING\")")]
            public static void MissingMethodWhenKnownExactType(string type)
            {
                var code = @"
namespace N
{
    public class C
    {
        public object? M(C c) => typeof(C).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(C).GetMethod(↓\"MISSING\")", type);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("typeof(C).GetMethod(↓\"MISSING\")")]
            [TestCase("c.GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("new C().GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("this.GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("GetType().GetMethod(↓\"MISSING\")")]
            public static void MissingMethodWhenSealed(string type)
            {
                var code = @"
namespace N
{
    public sealed class C
    {
        public object? M(C c) => typeof(C).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(C).GetMethod(↓\"MISSING\")", type);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void MissingMethodWhenStruct()
            {
                var code = @"
namespace N
{
    public struct C
    {
        public C(int _)
        {
            var methodInfo = typeof(C).GetMethod(↓""MISSING"");
        }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void MissingMethodWhenStatic()
            {
                var code = @"
namespace N
{
    public static class C
    {
        public static void M()
        {
            var methodInfo = typeof(C).GetMethod(↓""MISSING"");
        }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void MissingMethodWhenInterface()
            {
                var iC = @"
namespace N
{
    public interface IC
    {
    }
}";

                var code = @"
namespace N
{
    public class C
    {
        public C()
        {
            var methodInfo = typeof(IC).GetMethod(↓""MISSING"");
        }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, iC, code);
            }

            [TestCase("typeof(string).GetMethod(↓\"MISSING\")")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static)")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)")]
            [TestCase("typeof(string).GetMethod(↓\"MISSING\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, Type.DefaultBinder, Type.EmptyTypes, null)")]
            public static void MissingMethodNotInSource(string type)
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo? M(Type unused) => typeof(string).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(string).GetMethod(↓\"MISSING\")", type);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void MissingPropertySetAccessor()
            {
                var code = @"
namespace N
{
    public sealed class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(↓""set_P"");
        }

        public int P { get; }
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
    public sealed class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(↓""get_P"");
        }

        public int P { set { } }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
