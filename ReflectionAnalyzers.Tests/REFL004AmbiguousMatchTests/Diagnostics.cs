namespace ReflectionAnalyzers.Tests.REFL004AmbiguousMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL004AmbiguousMatch.Descriptor);

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

    public class C : Base
    {
        public C()
        {
            _ = typeof(C).GetProperty↓(""Item"");
        }

        public int this[int i] => 0;
    }
}".AssertReplace("GetProperty↓(\"Item\")", call);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, baseCode, code);
        }

        [Test]
        public void TwoIndexers()
        {
            var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public object Get => typeof(C).GetProperty↓(""Item"");

        public int this[int i] => 0;

        public int this[int i1, int i2] => 0;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void TwoNamedIndexers()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Runtime.CompilerServices;

    public class C
    {
        public object Get => typeof(C).GetProperty↓(""Bar"");

        [IndexerName(""Bar"")]
        public int this[int i] => 0;

        [IndexerName(""Bar"")]
        public int this[int i1, int i2] => 0;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void StaticAndInstanceConstructor()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class C
    {
        static C()
        {
        }

        public static object Get => typeof(C).GetConstructor↓(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null, Type.EmptyTypes, null);
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
