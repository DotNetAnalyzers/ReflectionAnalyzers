namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class OverloadResolution
        {
            [TestCase("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
            public static void InterfaceAndSame(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static int Static(int i) => i;

        public static IFormattable Static(IFormattable i) => i;
    }
}".AssertReplace("GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void ObjectAndInterface()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public static object Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static void Static(object _) { }

        public static void Static(IFormattable i) { }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
