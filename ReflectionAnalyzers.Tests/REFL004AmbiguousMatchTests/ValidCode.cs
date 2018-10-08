namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL004AmbiguousMatch.Descriptor;

        [TestCase("GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("GetMethod(nameof(this.ToString))")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicStaticInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicStaticInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance))")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void GetMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int PublicStatic(int value) => value;

        public static double PublicStatic(double value) => value;

        public static int PublicStaticInstance(int value) => value;

        public double PublicStaticInstance(double value) => value;

        public int PublicInstance(int value) => value;

        public double PublicInstance(double value) => value;

        public int PublicPrivateInstance(int value) => value;

        private double PublicPrivateInstance(double value) => value;
    }
}".AssertReplace("GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)", call);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { i.GetType() }, null)")]
        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new Type[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new Type[1] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.Instance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.Instance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { i.GetType() }, null)")]
        [TestCase("GetMethod(nameof(this.Instance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.Instance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[1] { typeof(int) }, null)")]
        public void OverloadsFilteredByType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo(int i)
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }

        public static double Static(int value) => value;

        public static double Static(double value) => value;

        public int Instance(int value) => value;

        public double Instance(double value) => value;
    }
}".AssertReplace("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetProperty(\"Item\", typeof(int), new[] { typeof(int) })")]
        [TestCase("GetProperty(\"Item\", typeof(int), new[] { typeof(int), typeof(int) })")]
        public void TwoIndexers(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public object Get => typeof(Foo).GetProperty(""Item"", typeof(int), new[] { typeof(int) });

        public int this[int i] => 0;

        public int this[int i1, int i2] => 0;
    }
}".AssertReplace("GetProperty(\"Item\", typeof(int), new[] { typeof(int) })", call);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetProperty(\"Bar\", typeof(int), new[] { typeof(int) })")]
        [TestCase("GetProperty(\"Bar\", typeof(int), new[] { typeof(int), typeof(int) })")]
        public void TwoNamedIndexers(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Runtime.CompilerServices;

    public class Foo
    {
        public object Get => typeof(Foo).GetProperty(""Bar"", typeof(int), new[] { typeof(int) });

        [IndexerName(""Bar"")]
        public int this[int i] => 0;

        [IndexerName(""Bar"")]
        public int this[int i1, int i2] => 0;
    }
}".AssertReplace("GetProperty(\"Bar\", typeof(int), new[] { typeof(int) })", call);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
