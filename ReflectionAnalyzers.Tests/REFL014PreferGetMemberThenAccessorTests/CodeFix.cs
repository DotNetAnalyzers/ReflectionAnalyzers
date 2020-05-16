namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseGetMemberThenAccessorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL014");

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                                              "GetProperty(nameof(this.PublicGetSet)).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)",        "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), Type.EmptyTypes, null).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                             "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                 "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                                              "GetProperty(nameof(this.PublicGetSet)).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                     "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new [] { typeof(int) }, null)", "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), Type.EmptyTypes, null).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                          "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                 "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public static void InstancePropertyInSameType(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }

        private int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", beforeExpression);
            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }

        private int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", afterExpression);
            var message = $"Prefer typeof(C).{afterExpression}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet)).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet)).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public static void StaticPropertyInSameType(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }

        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }

        private static int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod;
        }

        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }

        private static int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod", afterExpression);

            var message = $"Prefer typeof(C).{afterExpression}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                     "GetProperty(nameof(C.PublicGetSet)).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                     "GetProperty(nameof(C.PublicGetSet)).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public static void InstancePropertyInOtherType(string beforeExpression, string afterExpression)
        {
            var c = @"
namespace N
{
    public class C
    {
        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }
        
        private int PrivateGetSet { get; set; }
    }
}";

            var before = @"
namespace N
{
    using System.Reflection;

    class C2
    {
        public C2()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    class C2
    {
        public C2()
        {
            var methodInfo = typeof(C).GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", afterExpression);

            var message = $"Prefer typeof(C).{afterExpression}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { c, before }, after);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(C.PublicGetSet)).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(C.PublicGetSet)).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public static void StaticPropertyInOtherType(string beforeExpression, string afterExpression)
        {
            var c = @"
namespace N
{
    public static class C
    {
        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }
        
        private static int PrivateGetSet { get; set; }
    }
}";

            var before = @"
namespace N
{
    using System.Reflection;

    class C2
    {
        public C2()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    class C2
    {
        public C2()
        {
            var methodInfo = typeof(C).GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", afterExpression);

            var message = $"Prefer typeof(C).{afterExpression}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { c, before }, after);
        }

        [TestCase("GetMethod(\"get_P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public static async Task InvisibleProperty(string beforeExpression, string afterExpression)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(BinaryReferencedAssembly.C).GetMethod(""get_P"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }
}".AssertReplace("GetMethod(\"get_P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(BinaryReferencedAssembly.C).GetProperty(""P"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
    }
}".AssertReplace("GetProperty(\"P\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", afterExpression);

            var binaryReferencedCode = @"
namespace BinaryReferencedAssembly
{
    public class C
    {
        private int P { get; set; }
    }
}";
            var binaryReference = TestHelper.CompileBinaryReference(binaryReferencedCode);

            var solution = CodeFactory.CreateSolution(
                code,
                CodeFactory.DefaultCompilationOptions(new[] { Analyzer })
                           .WithMetadataImportOptions(MetadataImportOptions.Public),
                MetadataReferences.FromAttributes().Add(binaryReference));

            // To make sure the test is effective, assert that ReflectionAnalyzers *can’t* see C.P.
            var compilation = await solution.Projects.Single()
                                            .GetCompilationAsync()
                                            .ConfigureAwait(true);
            var fooType = compilation.GetTypeByMetadataName("BinaryReferencedAssembly.C");
            Assert.That(fooType.GetMembers(), Has.None.With.Property("Name")
                                                 .EqualTo("P"));

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, solution, after);
        }

        [TestCase("GetMethod(\"add_Public\")",                                                                                                                         "GetEvent(nameof(this.Public)).AddMethod")]
        [TestCase("GetMethod(\"add_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod")]
        [TestCase("GetMethod(\"add_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(EventHandler) }, null)",    "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod")]
        [TestCase("GetMethod(\"remove_Public\")",                                                                                                                      "GetEvent(nameof(this.Public)).RemoveMethod")]
        [TestCase("GetMethod(\"remove_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(EventHandler) }, null)", "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).RemoveMethod")]
        public static void InstanceEventInSameType(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).↓GetMethod(""add_Public"");
        }

        public event EventHandler Public;
    }
}".AssertReplace("GetMethod(\"add_Public\")", beforeExpression);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod;
        }

        public event EventHandler Public;
    }
}".AssertReplace("GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod", afterExpression);

            var message = $"Prefer typeof(C).{afterExpression}.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [TestCase("GetMethod(\"get_Current\")",                                                                          "GetProperty(nameof(IEnumerator.Current)).GetMethod")]
        [TestCase("GetMethod(\"get_Current\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(IEnumerator.Current), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        //// ReSharper disable once InconsistentNaming
        public static void IEnumeratorGetCurrent(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System.Collections;
    using System.Reflection;

    public class C
    {
        public void M(object value)
        {
            _ = typeof(IEnumerator).GetMethod(""get_Current"");
        }
    }
}".AssertReplace("GetMethod(\"get_Current\")", beforeExpression);
            var after = @"
namespace N
{
    using System.Collections;
    using System.Reflection;

    public class C
    {
        public void M(object value)
        {
            _ = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(IEnumerator.Current)).GetMethod", afterExpression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)",                             "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod")]
        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)",                             "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).SetMethod")]
        [TestCase("GetMethod(\"set_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public static void AggregateException(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", beforeExpression);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        }
    }
}".AssertReplace("GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod", afterExpression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("GetMethod(\"get_Item\")",                                                                                                                          "GetProperty(\"Item\").GetMethod")]
        [TestCase("GetMethod(\"get_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                 "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)",              "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).GetMethod")]
        [TestCase("GetMethod(\"set_Item\")",                                                                                                                          "GetProperty(\"Item\").SetMethod")]
        [TestCase("GetMethod(\"set_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                 "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(int) }, null)", "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).SetMethod")]
        public static void Indexer(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetMethod(""get_Item"");

        public int this[int i]
        {
            get => this.ints[i];
            set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetMethod(\"get_Item\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetProperty(""Item"").GetMethod;

        public int this[int i]
        {
            get => this.ints[i];
            set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetProperty(\"Item\").GetMethod", afterExpression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("GetMethod(\"get_Item\")",                                                                                                                             "GetProperty(\"Item\").GetMethod")]
        [TestCase("GetMethod(\"get_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                    "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)",                 "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).GetMethod")]
        [TestCase("GetMethod(\"set_Item\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                 "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_Item\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(int) }, null)", "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).SetMethod")]
        public static void IndexerPrivateSet(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetMethod(""get_Item"");

        public int this[int i]
        {
            get => this.ints[i];
            private set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetMethod(\"get_Item\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetProperty(""Item"").GetMethod;

        public int this[int i]
        {
            get => this.ints[i];
            private set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetProperty(\"Item\").GetMethod", afterExpression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("GetMethod(\"get_Foo\")",                                                                                                                          "GetProperty(\"Foo\").GetMethod")]
        [TestCase("GetMethod(\"get_Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                 "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)",              "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).GetMethod")]
        [TestCase("GetMethod(\"set_Foo\")",                                                                                                                          "GetProperty(\"Foo\").SetMethod")]
        [TestCase("GetMethod(\"set_Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                 "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(int) }, null)", "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null).SetMethod")]
        public static void NamedIndexer(string beforeExpression, string afterExpression)
        {
            var before = @"
namespace N
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetMethod(""get_Foo"");

        [IndexerName(""Foo"")]
        public int this[int i]
        {
            get => this.ints[i];
            set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetMethod(\"get_Foo\")", beforeExpression);

            var after = @"
namespace N
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C
    {
        private readonly int[] ints = System.Array.Empty<int>();

        public object Get => typeof(C).GetProperty(""Item"").GetMethod;

        [IndexerName(""Foo"")]
        public int this[int i]
        {
            get => this.ints[i];
            set => this.ints[i] = value;
        }
    }
}".AssertReplace("GetProperty(\"Item\").GetMethod", afterExpression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static async Task ReferencesMemberThatAnalyzerCannotSee()
        {
            var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(BinaryReferencedAssembly.C1).GetMethod(""add_E"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }
}";
            var after = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public object Get => typeof(BinaryReferencedAssembly.C1).GetEvent(""E"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod;
    }
}";

            var binaryReference = TestHelper.CompileBinaryReference(@"
namespace N.BinaryReferencedAssembly
{
    using System;

    public interface IC1
    {
        event EventHandler E;
    }

    public class C1 : IC1
    {
        #pragma warning disable CS0067
        internal event EventHandler E;

        event EventHandler IC1.E
        {
            add { }
            remove { }
        }
    }
}");

            var solution = CodeFactory.CreateSolution(
                code,
                CodeFactory.DefaultCompilationOptions(new[] { Analyzer })
                           .WithMetadataImportOptions(MetadataImportOptions.Public),
                MetadataReferences.FromAttributes().Add(binaryReference));

            // To make sure the test is effective, assert that ReflectionAnalyzers *can’t* see Foo.Bar.
            var compilation = await solution.Projects.Single()
                                            .GetCompilationAsync()
                                            .ConfigureAwait(true);
            var fooType = compilation.GetTypeByMetadataName("N.BinaryReferencedAssembly.C1");
            Assert.That(fooType.GetMembers(), Has.None.With.Property("Name")
                                                 .EqualTo("Bar"));

            var message = @"Prefer typeof(BinaryReferencedAssembly.C1).GetEvent(""E"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), solution, after);
        }

        [Test]
        public static void InNestedType()
        {
            var before = @"
namespace N
{
    using System.Reflection;

    class C
    {
        protected bool P { get; }

        class Nested
        {
            object Get => typeof(C).↓GetMethod(""get_P"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";

            var after = @"
namespace N
{
    using System.Reflection;

    class C
    {
        protected bool P { get; }

        class Nested
        {
            object Get => typeof(C).GetProperty(nameof(C.P), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void InNestedTypeWhenInheritance()
        {
            var @base = @"
namespace N
{
    class Base
    {
        protected bool P { get; }
    }
}";

            var before = @"
namespace N
{
    using System.Reflection;

    class C : Base
    {
        class Nested
        {
            object Get => typeof(C).↓GetMethod(""get_P"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";

            var after = @"
namespace N
{
    using System.Reflection;

    class C : Base
    {
        class Nested
        {
            object Get => typeof(C).GetProperty(nameof(C.P), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { @base, before }, after);
        }
    }
}
