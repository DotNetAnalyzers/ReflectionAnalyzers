namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("REFL003");

        [TestCase("typeof(Foo).GetMethod(nameof(PublicStatic))")]
        [TestCase("typeof(Foo).GetMethod(nameof(ReferenceEquals))")]
        [TestCase("typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.PublicInstance))")]
        [TestCase("typeof(Foo).GetMethod(nameof(PublicInstance))")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString))")]
        [TestCase("typeof(Foo).GetMethod(nameof(ToString))")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("typeof(Foo).GetMethod(nameof(PrivateStatic))")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.PrivateInstance))")]
        [TestCase("typeof(Foo).GetMethod(nameof(PrivateInstance))")]
        [TestCase("typeof(string).GetMethod(nameof(string.Clone))")]
        [TestCase("typeof(string).GetMethod(\"op_Equality\")")]
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
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }

        public static int PublicStatic() => 0;

        public int PublicInstance() => 0;

        private static int PrivateStatic() => 0;

        private int PrivateInstance() => 0;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(this.ToString))", call);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static)")]
        [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public void ExcludeNonPublicNotInSource(string invocation)
        {
        var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(""MISSING"", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}".AssertReplace("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static)", invocation);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void GetToStringOverridden()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }

        public override string ToString() => base.ToString();
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void GetToStringShadowing()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.ToString));
        }

        public new string ToString() => base.ToString();
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void GetOverloadedMethodInSameType()
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(this.Bar));
        }

        public void Bar()
        {
        }

        public int Bar(int i) => i;
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void GetMethodWhenUnknownType()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo(Type type)
        {
            var methodInfo = type.GetMethod(""Bar"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("get_Bar")]
        [TestCase("set_Bar")]
        public void GetPropertyMethods(string name)
        {
            var code = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(""get_Bar"");
        }

        public int Bar { get; set; }
    }
}".AssertReplace("get_Bar", name);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
