namespace ReflectionAnalyzers.Tests.REFL003MemberDoesNotExistTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        internal class GetMethod
        {
            [TestCase("typeof(C).GetMethod(nameof(PublicStatic))")]
            [TestCase("typeof(C).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
            [TestCase("typeof(C).GetMethod(nameof(this.PublicInstance))")]
            [TestCase("typeof(C).GetMethod(nameof(PublicInstance))")]
            [TestCase("typeof(C).GetMethod(nameof(this.ToString))")]
            [TestCase("typeof(C).GetMethod(nameof(ToString))")]
            [TestCase("typeof(C).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("typeof(C).GetMethod(nameof(PrivateStatic))")]
            [TestCase("typeof(C).GetMethod(nameof(this.PrivateInstance))")]
            [TestCase("typeof(C).GetMethod(nameof(PrivateInstance))")]
            [TestCase("typeof(string).GetMethod(nameof(string.Clone))")]
            [TestCase("typeof(string).GetMethod(\"op_Equality\")")]
            public void Vanilla(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString));
        }

        public static int PublicStatic() => 0;

        public int PublicInstance() => 0;

        private static int PrivateStatic() => 0;

        private int PrivateInstance() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(this.ToString))", call);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("new C().GetType().GetMethod(\"MISSING\")")]
            [TestCase("this.GetType().GetMethod(\"MISSING\")")]
            [TestCase("GetType().GetMethod(\"MISSING\")")]
            public void MissingMethod(string type)
            {
                var code = @"
namespace RoslynSandbox
{
    public class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(C));
        }
    }
}".AssertReplace("typeof(C).GetMethod(nameof(C))", type);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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

    class C
    {
        public C()
        {
            var methodInfo = typeof(string).GetMethod(""MISSING"", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}".AssertReplace("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static)", invocation);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void ToStringOverridden()
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString));
        }

        public override string ToString() => base.ToString();
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void ToStringShadowing()
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString));
        }

        public new string ToString() => base.ToString();
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void OverloadedMethodInSameType()
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar));
        }

        public void Bar()
        {
        }

        public int Bar(int i) => i;
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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

    class C
    {
        public C()
        {
            var methodInfo = typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void UnknownType()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public C(Type type)
        {
            var methodInfo = type.GetMethod(""Bar"");
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void TypeParameter()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public MethodInfo Bar<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : C", "GetMethod(nameof(this.Baz))")]
            [TestCase("where T : C", "GetMethod(nameof(this.Baz), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            public void ConstrainedTypeParameter(string constraint, string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo Bar<T>()
            where T : C
        {
            return typeof(T).GetMethod(nameof(this.Baz));
        }

        public int Baz() => 0;
    }
}".AssertReplace("where T : C", constraint)
      .AssertReplace("GetMethod(nameof(this.Baz))", call);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public void Generic()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public C()
        {
            _ = typeof(C).GetMethod(nameof(C.Id), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public T Id<T>(T value) => value;
    }
}";
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("get_Bar")]
            [TestCase("set_Bar")]
            public void PropertyAccessors(string name)
            {
                var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(""get_Bar"");
        }

        public int Bar { get; set; }
    }
}".AssertReplace("get_Bar", name);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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

    class C
    {
        public C(Delegate foo)
        {
            var methodInfo = foo.GetType().GetMethod(""Invoke"");
        }
    }
}".AssertReplace("Delegate", type);
                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance")]
            [TestCase("Static",           "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
            [TestCase("Static",           "BindingFlags.NonPublic | BindingFlags.Static")]
            [TestCase("ReferenceEquals",  "BindingFlags.Public | BindingFlags.Static")]
            [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static")]
            [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance")]
            [TestCase("this.Public",      "BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
            [TestCase("this.Public",      "BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly")]
            [TestCase("this.ToString",    "BindingFlags.Public")]
            [TestCase("this.ToString",    "BindingFlags.NonPublic | BindingFlags.Static")]
            [TestCase("this.ToString",    "BindingFlags.Public | BindingFlags.Static")]
            [TestCase("this.GetHashCode", "BindingFlags.Public")]
            [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
            [TestCase("this.GetHashCode", "BindingFlags.NonPublic | BindingFlags.Static")]
            [TestCase("this.GetHashCode", "BindingFlags.Public | BindingFlags.Static")]
            [TestCase("this.Private",     "BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly")]
            [TestCase("this.Private",     "BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly")]
            public void WrongFlags(string method, string flags)
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Static);
        }

        public static int Static() => 0;

        public int Public() => 0;

        public override string ToString() => string.Empty;

        private int Private() => 0;
    }
}".AssertReplace("nameof(this.Public)", $"nameof({method})")
  .AssertReplace("BindingFlags.Public | BindingFlags.Static", flags);

                AnalyzerAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
