namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new NameofAnalyzer();
        private static readonly CodeFixProvider Fix = new NameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL016UseNameof.DiagnosticId);

        [Test]
        public void GetPropertyInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetProperty(""Bar"");
        }

         public int Bar { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
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

    class Foo
    {
        public Foo()
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

    class Foo
    {
        public Foo()
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
    class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetProperty(""Bar"");
        }

         public static int Bar { get; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
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
    class Foo
    {
        public Foo()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty(""Foo"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty(nameof(anon.Foo));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Use nameof(anon.Foo).");
        }

        [Test]
        public void TypeofDictionaryGetMethodStringLiteral()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(↓""Add"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Use nameof(Dictionary<string, object>.Add).");
        }

        [Test]
        public void TypeOfDictionaryGetMethodNameOfStaticAdd()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(↓Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Use nameof(Dictionary<string, object>.Add).");
        }

        [Test]
        public void TypeOfConsoleGetMethodNameOfStaticWriteLine()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Console).GetMethod(nameof(WriteLine), Type.EmptyTypes);
        }

        public bool WriteLine { get; set; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Console).GetMethod(nameof(Console.WriteLine), Type.EmptyTypes);
        }

        public bool WriteLine { get; set; }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void TypeOfDictionaryGetMethodNameOfHashSetAdd()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void ThisGetTypeGetMethodNameOfHashSetAddStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void ThisGetTypeGetMethodNameOfHashSetAddInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(this.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void ThisGetTypeGetMethodNameOfHashSetAddUnderscore()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = GetType().GetMethod(nameof(Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
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

    public class Foo
    {
        public Foo()
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

    public class Foo
    {
        public Foo()
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

    public class Foo
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
            var member = typeof(Foo).GetNestedType(↓""Class"", BindingFlags.Public);
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
            var member = typeof(Foo).GetNestedType(nameof(Foo.Class), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(Foo.Class)", $"nameof(Foo.{type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooCode, testCode }, fixedCode);
        }

        [Test]
        public void AggregateExceptionMessage()
        {
            var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetProperty(""Message"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetProperty(nameof(Exception.Message), BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
