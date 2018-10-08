namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        public class OverloadResolution
        {
            [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
            public void InterfaceAndSame(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public static object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static int Static(int i) => i;

        public static IFormattable Static(IFormattable i) => i;
    }
}".AssertReplace("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void ObjectAndInterface()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public static object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static void Static(object _) { }

        public static void Static(IFormattable i) { }
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
