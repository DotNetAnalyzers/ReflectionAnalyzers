namespace ReflectionAnalyzers.Tests.REFL014PreferGetMemberThenAccessorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseGetMemberThenAccessorFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL014PreferGetMemberThenAccessor.DiagnosticId);

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                                       "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                              "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)", "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), Type.EmptyTypes, null).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                      "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                         "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                          "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                                       "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                              "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)", "GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), Type.EmptyTypes, null).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                   "GetProperty(nameof(this.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                         "GetProperty(nameof(this.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                          "GetProperty(nameof(this.PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void InstancePropertyInSameType(string before, string after)
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
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }

        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }

        private int PrivateGetSet { get; set; }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);
            var fixedCode = @"
namespace RoslynSandbox
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
}".AssertReplace("GetProperty(nameof(this.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);
            var message = $"Prefer typeof(C).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(nameof(PrivateGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public void StaticPropertyInSameType(string before, string after)
        {
            var code = @"
namespace RoslynSandbox
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
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);

            var fixedCode = @"
namespace RoslynSandbox
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
}".AssertReplace("GetProperty(nameof(PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod", after);

            var message = $"Prefer typeof(C).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                     "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                     "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void InstancePropertyInOtherType(string before, string after)
        {
            var foo = @"
namespace RoslynSandbox
{
    public class C
    {
        public int PublicGetSet { get; set; }

        public int PublicGetInternalSet { get; internal set; }

        internal int InternalGetSet { get; set; }
        
        private int PrivateGetSet { get; set; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(C).GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);

            var message = $"Prefer typeof(C).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { foo, code }, fixedCode);
        }

        [TestCase("GetMethod(\"get_PublicGetSet\")",                                                                                   "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PublicGetInternalSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",    "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"get_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\")",                                                                                   "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetSet\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)",            "GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PublicGetInternalSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)", "GetProperty(nameof(C.PublicGetInternalSet), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_InternalGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",       "GetProperty(nameof(C.InternalGetSet), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        [TestCase("GetMethod(\"set_PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)",        "GetProperty(\"PrivateGetSet\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).SetMethod")]
        public void StaticPropertyInOtherType(string before, string after)
        {
            var foo = @"
namespace RoslynSandbox
{
    public static class C
    {
        public static int PublicGetSet { get; set; }

        public static int PublicGetInternalSet { get; internal set; }

        internal static int InternalGetSet { get; set; }
        
        private static int PrivateGetSet { get; set; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(C).↓GetMethod(""get_PublicGetSet"");
        }
    }
}".AssertReplace("GetMethod(\"get_PublicGetSet\")", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Bar
    {
        public Bar()
        {
            var methodInfo = typeof(C).GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}".AssertReplace("GetProperty(nameof(C.PublicGetSet), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod", after);

            var message = $"Prefer typeof(C).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { foo, code }, fixedCode);
        }

        [TestCase("GetMethod(\"add_Public\")",                                                                                                                         "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod")]
        [TestCase("GetMethod(\"add_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)",                                                "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod")]
        [TestCase("GetMethod(\"add_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(EventHandler) }, null)",    "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod")]
        [TestCase("GetMethod(\"remove_Public\")",                                                                                                                      "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).RemoveMethod")]
        [TestCase("GetMethod(\"remove_Public\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(EventHandler) }, null)", "GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).RemoveMethod")]
        public void InstanceEventInSameType(string before, string after)
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
            var methodInfo = typeof(C).↓GetMethod(""add_Public"");
        }

        public event EventHandler Public;
    }
}".AssertReplace("GetMethod(\"add_Public\")", before);

            var fixedCode = @"
namespace RoslynSandbox
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
}".AssertReplace("GetEvent(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AddMethod", after);

            var message = $"Prefer typeof(C).{after}.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [Test]
        //// ReSharper disable once InconsistentNaming
        public void IEnumeratorGetCurrent()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections;

    public class C
    {
        public void Meh(object value)
        {
            _ = typeof(IEnumerator).GetMethod(""get_Current"");
        }
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections;
    using System.Reflection;

    public class C
    {
        public void Meh(object value)
        {
            _ = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod;
        }
    }
}";

            var message = "Prefer typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod.";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)",                             "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod")]
        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)",                             "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).SetMethod")]
        [TestCase("GetMethod(\"set_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", "GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void AggregateException(string before, string after)
        {
            var code = @"
namespace RoslynSandbox.Dump
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
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", before);

            var fixedCode = @"
namespace RoslynSandbox.Dump
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
}".AssertReplace("GetProperty(\"InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod", after);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("GetMethod(\"get_Item\")",                                                                                                             "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_Item\")",                                                                                                             "GetProperty(\"Item\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void Indexer(string before, string after)
        {
            var code = @"
namespace RoslynSandbox.Dump
{
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
}".AssertReplace("GetMethod(\"get_Item\")", before);

            var fixedCode = @"
namespace RoslynSandbox.Dump
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
}".AssertReplace("GetProperty(\"Item\").GetMethod", after);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("GetMethod(\"get_Foo\")",                                                                                                             "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetMethod")]
        [TestCase("GetMethod(\"set_Foo\")",                                                                                                             "GetProperty(\"Foo\", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetMethod")]
        public void NamedIndexer(string before, string after)
        {
            var code = @"
namespace RoslynSandbox.Dump
{
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
}".AssertReplace("GetMethod(\"get_Foo\")", before);

            var fixedCode = @"
namespace RoslynSandbox.Dump
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
}".AssertReplace("GetProperty(\"Item\").GetMethod", after);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
