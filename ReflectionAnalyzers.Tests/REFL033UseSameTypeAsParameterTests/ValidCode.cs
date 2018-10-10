namespace ReflectionAnalyzers.Tests.REFL033UseSameTypeAsParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL033UseSameTypeAsParameter.Descriptor;

        [Test]
        public void ExactInterfaceParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);

        public static IComparable Static(IComparable i) => i;
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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

    class Foo
    {
        public object Get => typeof(Control).GetMethod(
            nameof(Control.CreateControl),
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(bool) },
            null);
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
