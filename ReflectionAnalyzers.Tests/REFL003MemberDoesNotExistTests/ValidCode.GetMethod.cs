namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        internal class GetMethod
        {
            [TestCase("typeof(Foo).GetMethod(nameof(PublicStatic))")]
            [TestCase("typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
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
            public void Vanilla(string call)
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

            [TestCase("new Foo().GetType().GetMethod(\"MISSING\")")]
            [TestCase("this.GetType().GetMethod(\"MISSING\")")]
            [TestCase("GetType().GetMethod(\"MISSING\")")]
            public void MissingMethod(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetMethod(nameof(Foo));
        }
    }
}".AssertReplace("typeof(Foo).GetMethod(nameof(Foo))", type);
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
            public void ToStringOverridden()
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
            public void ToStringShadowing()
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
            public void OverloadedMethodInSameType()
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

            [TestCase("GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
            public void ExplicitImplementation(string call)
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
            var methodInfo = typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public void UnknownType()
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
            public void TypeParameter()
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

            [TestCase("where T : Foo", "GetMethod(nameof(this.Baz))")]
            [TestCase("where T : Foo", "GetMethod(nameof(this.Baz), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : Foo, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            public void ConstrainedTypeParameter(string constraint, string call)
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

            [Test]
            public void Generic()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Foo.Id), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public T Id<T>(T value) => value;
    }
}";
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("get_Bar")]
            [TestCase("set_Bar")]
            public void PropertyAccessors(string name)
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

            [TestCase("Delegate")]
            [TestCase("Action")]
            [TestCase("Action<int>")]
            public void DelegateInvoke(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    class Foo
    {
        public Foo(Delegate foo)
        {
            var methodInfo = foo.GetType().GetMethod(""Invoke"");
        }
    }
}".AssertReplace("Delegate", type);
                AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
