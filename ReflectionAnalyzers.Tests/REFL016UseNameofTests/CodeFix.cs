namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new NameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL016UseNameof.DiagnosticId);

        [Test]
        public void GetPropertyInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var member = this.GetType().GetProperty(""Bar"");
        }

         public int Bar { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var member = this.GetType().GetProperty(nameof(this.Bar));
        }

         public int Bar { get; }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void GetPropertyInstanceWithTrivia()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = this.GetType().GetProperty(
  /* trivia1 */ ""Bar""  ,    // trivia2
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

         public int Bar { get; }
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
            var member = this.GetType().GetProperty(
  /* trivia1 */ nameof(this.Bar)  ,    // trivia2
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

         public int Bar { get; }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void GetPropertyStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var member = this.GetType().GetProperty(""Bar"");
        }

         public static int Bar { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var member = this.GetType().GetProperty(nameof(Bar));
        }

         public static int Bar { get; }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnonymousType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var anon = new { C = 1 };
            var member = anon.GetType().GetProperty(""C"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var anon = new { C = 1 };
            var member = anon.GetType().GetProperty(nameof(anon.C));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Use nameof(anon.C).");
        }

        [Test]
        public void TypeofDictionaryGetMethodStringLiteral()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(↓""Add"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Use nameof(Dictionary<string, object>.Add).");
        }

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public void GetNestedTypePrivateInSameType(string type)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetNestedType(↓""Class"", BindingFlags.NonPublic);
        }

        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}".AssertReplace("GetNestedType(↓\"Class\", BindingFlags.NonPublic)", $"GetNestedType(↓\"{type}\", BindingFlags.NonPublic)");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetNestedType(nameof(Class), BindingFlags.NonPublic);
        }

        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}".AssertReplace("nameof(Class)", $"nameof({type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public void GetNestedTypePublicInOtherType(string type)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public class Class { }

        public enum Enum { }

        public interface Interface { }

        public struct Struct { }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Bar
    {
        public Bar()
        {
            var member = typeof(C).GetNestedType(↓""Class"", BindingFlags.Public);
        }
    }
}".AssertReplace("GetNestedType(↓\"Class\", BindingFlags.Public)", $"GetNestedType(↓\"{type}\", BindingFlags.Public)");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Bar
    {
        public Bar()
        {
            var member = typeof(C).GetNestedType(nameof(C.Class), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(C.Class)", $"nameof(C.{type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooCode, testCode }, fixedCode);
        }

        [Test]
        public void AggregateExceptionMessage()
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
            var member = typeof(AggregateException).GetProperty(""Message"", BindingFlags.Public | BindingFlags.Instance);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(nameof(Exception.Message), BindingFlags.Public | BindingFlags.Instance);
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void SystemWindowsFormsControlCreateControl()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public object Get => typeof(Control).GetMethod(""CreateControl"", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public object Get => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void NullableGetProperty()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public object Get => typeof(int?).GetProperty(""Value"");
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public object Get => typeof(int?).GetProperty(nameof(Nullable<int>.Value));
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ValueTupleGetFieldItem1()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public object Get => typeof((int, double)).GetField(""Item1"");
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public object Get => typeof((int, double)).GetField(nameof(ValueTuple<int, double>.Item1));
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ValueTupleGetFieldRest()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public object Get => typeof((int, int, int, int, int, int, int, int)).GetField(""Rest"");
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public object Get => typeof((int, int, int, int, int, int, int, int)).GetField(nameof(ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>.Rest));
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ProtectedMemberInBase()
        {
            var baseCode = @"
namespace RoslynSandbox
{
    class BaseClass
    {
        protected void ProtectedMember() { }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    class C : BaseClass
    {
        public object Get => typeof(BaseClass).GetMethod(↓""ProtectedMember"");
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C : BaseClass
    {
        public object Get => typeof(BaseClass).GetMethod(nameof(this.ProtectedMember));
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { baseCode, code }, fixedCode);
        }
    }
}
