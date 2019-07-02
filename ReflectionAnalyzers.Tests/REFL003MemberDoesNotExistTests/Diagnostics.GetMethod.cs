namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class GetMethod
        {
            [TestCase("typeof(C).GetMethod(↓\"MISSING\")")]
            public static void MissingMethodWhenKnownExactType(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public object Bar(C foo) => typeof(C).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(C).GetMethod(↓\"MISSING\")", type);

                var message = "The type RoslynSandbox.C does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("typeof(C).GetMethod(↓\"MISSING\")")]
            [TestCase("foo.GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("new C().GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("this.GetType().GetMethod(↓\"MISSING\")")]
            [TestCase("GetType().GetMethod(↓\"MISSING\")")]
            public static void MissingMethodWhenSealed(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    public sealed class C
    {
        public object Bar(C foo) => typeof(C).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(C).GetMethod(↓\"MISSING\"", type);

                var message = "The type RoslynSandbox.C does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void MissingMethodWhenStruct()
            {
                var code = @"
namespace RoslynSandbox
{
    public struct C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(↓""MISSING"");
        }
    }
}";
                var message = "The type RoslynSandbox.C does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void MissingMethodWhenStatic()
            {
                var code = @"
namespace RoslynSandbox
{
    public static class C
    {
        public void Bar()
        {
            var methodInfo = typeof(C).GetMethod(↓""MISSING"");
        }
    }
}";
                var message = "The type RoslynSandbox.C does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void MissingMethodWhenInterface()
            {
                var interfaceCode = @"
namespace RoslynSandbox
{
    public interface IC
    {
    }
}";

                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var methodInfo = typeof(IC).GetMethod(↓""MISSING"");
        }
    }
}";
                var message = "The type RoslynSandbox.IC does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), interfaceCode, code);
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
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public object Get => typeof(string).GetMethod(↓""MISSING"");
    }
}".AssertReplace("typeof(string).GetMethod(↓\"MISSING\")", type);
                var message = "The type string does not have a member named MISSING.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void MissingPropertySetAccessor()
            {
                var code = @"
namespace RoslynSandbox
{
    public sealed class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(↓""set_Bar"");
        }

        public int Bar { get; }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void MissingPropertyGetAccessor()
            {
                var code = @"
namespace RoslynSandbox
{
    public sealed class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(↓""get_Bar"");
        }

        public int Bar { set; }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
