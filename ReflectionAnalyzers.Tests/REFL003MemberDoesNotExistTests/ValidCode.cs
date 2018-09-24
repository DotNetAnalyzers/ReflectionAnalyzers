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
        public void GetMethodExcludeNonPublicNotInSource(string invocation)
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
        public void GetGetMethodToStringOverridden()
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
        public void GetMethodToStringShadowing()
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
        public void GetMethodOverloadedMethodInSameType()
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

        [Test]
        public void GetMethodWhenTypeParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public MethodInfo Bar<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("where T : Foo",               "GetMethod(nameof(this.Baz))")]
        [TestCase("where T : Foo",               "GetMethod(nameof(this.Baz), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : IConvertible",      "GetMethod(nameof(IConvertible.ToString))")]
        [TestCase("where T : IConvertible",      "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : IConvertible",      "GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("where T : IConvertible",      "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
        [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
        public void GetMethodWhenConstrainedTypeParameter(string constraint, string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class Foo
    {
        public MethodInfo Bar<T>()
            where T : Foo
        {
            return typeof(T).GetMethod(nameof(this.Baz));
        }

        public int Baz() => 0;
    }
}".AssertReplace("where T : Foo", constraint)
  .AssertReplace("GetMethod(nameof(this.Baz))", call);
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

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void AggregateExceptionGetInnerExceptionCount(string call)
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
            var member = typeof(AggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void SubclassAggregateExceptionGetInnerExceptionCount(string call)
        {
            var exception = @"
namespace RoslynSandbox
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }
}";
            var code = @"
namespace RoslynSandbox.Dump
{
    using System;
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var member = typeof(CustomAggregateException).GetMethod(""get_InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}".AssertReplace("GetMethod(\"get_InnerExceptionCount\", BindingFlags.NonPublic | BindingFlags.Instance)", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, exception, code);
        }
    }
}
