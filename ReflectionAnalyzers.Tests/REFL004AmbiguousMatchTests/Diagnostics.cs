namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL004");

        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.Instance), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void GetMethodPublicPrivateOverloads(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod↓(nameof(this.ToString));
        }

        public static double Static(int value) => value;

        public int Instance(int value) => value;

        private static double Static(double value) => value;

        private double Instance(double value) => value;
    }
}".AssertReplace("GetMethod↓(nameof(this.ToString))", call);
            var message = "More than one member is matching the criteria.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetMethod↓(nameof(Static))")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(Static), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod↓(nameof(this.Instance))")]
        [TestCase("GetMethod↓(nameof(this.Instance), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void GetMethodPublicOverloads(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod↓(nameof(this.ToString));
        }

        public static double Static(int value) => value;

        public static double Static(double value) => value;

        public int Instance(int value) => value;

        public double Instance(double value) => value;
    }
}".AssertReplace("GetMethod↓(nameof(this.ToString))", call);
            var message = "More than one member is matching the criteria.";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [TestCase("GetMethod↓(\"op_Explicit\")")]
        [TestCase("GetMethod↓(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static)")]
        public void OverloadedOperatorExplicit(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        void M()
        {
            typeof(C).GetMethod↓(""op_Explicit"");
        }

        public static explicit operator int(C c) => default;
        public static explicit operator C(int c) => default;
    }
}".AssertReplace("GetMethod↓(\"op_Explicit\")", call);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetProperty↓(\"Item\")")]
        [TestCase("GetProperty↓(\"Item\", BindingFlags.Public | BindingFlags.Instance)")]
        public void IndexerAndPropertyNamedItem(string call)
        {
            var baseCode = @"
namespace RoslynSandbox
{
    public class Base
    {
        public int Item { get; }
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : Base
    {
        public Foo()
        {
            _ = typeof(Foo).GetProperty↓(""Item"");
        }

        public int this[int i] => 0;
    }
}".AssertReplace("GetProperty↓(\"Item\")", call);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, baseCode, code);
        }
    }
}
