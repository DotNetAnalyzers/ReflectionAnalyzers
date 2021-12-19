namespace ReflectionAnalyzers.Tests.REFL033UseSameTypeAsParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL033UseSameTypeAsParameter;

        [Test]
        public static void ExactInterfaceParameter()
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MemberInfo? Get => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);

        public static IComparable Static(IComparable i) => i;
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void NullableParameter()
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MemberInfo? Get => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);

        public static IComparable? Static(IComparable? i) => i;
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void SystemWindowsFormsControlCreateControl()
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public object? Get => typeof(Control).GetMethod(
            nameof(Control.CreateControl),
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(bool) },
            null);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
