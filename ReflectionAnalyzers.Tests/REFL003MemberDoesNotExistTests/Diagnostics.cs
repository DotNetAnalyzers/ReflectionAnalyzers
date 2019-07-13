namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL003MemberDoesNotExist.Descriptor);

        [Test]
        public static void GetPropertyAnonymousType()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var anon = new { C = 1 };
            var member = anon.GetType().GetProperty(↓""MISSING"");
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void GetMissingPropertyThenNullCheck()
        {
            var code = @"
namespace RoslynSandbox
{
    public sealed class C
    {
        public C(C c)
        {
            var property = c.GetType().GetProperty(""P"");
            if (property != null)
            {
            }
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void SubclassAggregateExceptionGetFieldDeclaredOnly()
        {
            var exception = @"
namespace RoslynSandbox
{
    using System;

    public sealed class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(CustomAggregateException).GetField(↓""MISSING"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, exception, code);
        }

        [TestCase("GetNestedType(↓\"Generic\", BindingFlags.Public)")]
        [TestCase("GetNestedType(↓nameof(Generic<int>), BindingFlags.Public)")]
        [TestCase("GetNestedType(↓\"Generic`2\", BindingFlags.Public)")]
        public static void GetNestedType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetNestedType(↓nameof(Generic<int>), BindingFlags.Public);
        }

        public class Generic<T>
        {
        }
    }
}".AssertReplace("GetNestedType(↓nameof(Generic<int>), BindingFlags.Public)", call);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetProperty(↓\"Item\")")]
        [TestCase("GetProperty(↓\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void NamedIndexer(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public sealed class C
    {
        public C()
        {
            _ = typeof(C).GetProperty(""Item"");
        }

        [IndexerName(""Bar"")]
        public int this[int i] => 0;
    }
}".AssertReplace("GetProperty(\"Item\")", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void GetTupleFieldItem1ByName()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => Create().GetType().GetField(↓""a"");


        static (int a, int b) Create() => default;
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("\"a1\"")]
        [TestCase("\"a7\"")]
        [TestCase("\"a8\"")]
        public static void GetTupleFieldItem7ByName(string field)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => Create().GetType().GetField(↓""a7"");


        static (int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8) Create() => default;
    }
}".AssertReplace("\"a7\"", field);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(↓\"get_P\")")]
        public static void MissingGetter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).GetProperty(nameof(P)).↓GetMethod;


        public int P { set { } }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(P)).↓GetMethod", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(C).GetMethod(↓\"set_P\")")]
        public static void MissingSetter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public object Get => typeof(C).GetProperty(nameof(P)).↓SetMethod;


        public int P { get; }
    }
}".AssertReplace("typeof(C).GetProperty(nameof(P)).↓SetMethod", call);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
