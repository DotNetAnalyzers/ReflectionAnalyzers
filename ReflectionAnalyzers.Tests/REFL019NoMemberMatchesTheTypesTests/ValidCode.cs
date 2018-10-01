namespace ReflectionAnalyzers.Tests.REFL019NoMemberMatchesTheTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL019NoMemberMatchesTheTypes.Descriptor);

        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)")]
        [TestCase("typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Instance | BindingFlags.Static |BindingFlags.Public | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Private), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(IConvertible).GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(IConvertible).GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(IFormatProvider) }, null)")]
        public void GetMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)", call);
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        public void GetMethodOneParameter(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static int Static(int i) => i;

        public int Public(int i) => i;

        private int Private(int i) => i;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("typeof(Foo).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        public void GetMethodOneParameterOverloadResolution(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public object Get() => typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetConstructor(new[] { typeof(int) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
        public void GetConstructor(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo(int value)
        {
            var ctor = typeof(Foo).GetConstructor(new[] { typeof(int) });
        }
    }
}".AssertReplace("GetConstructor(new[] { typeof(int) })", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetConstructor(Type.EmptyTypes)")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)")]
        [TestCase("GetConstructor(new[] { typeof(int) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null)")]
        [TestCase("GetConstructor(new[] { typeof(double) })")]
        [TestCase("GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null)")]
        public void GetConstructorWhenOverloaded(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var ctor = typeof(Foo).GetConstructor(Type.EmptyTypes);
        }

        public Foo(int value)
        {
        }

        public Foo(double value)
        {
        }
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
