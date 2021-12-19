namespace ReflectionAnalyzers.Tests.REFL009MemberCantBeFoundTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class GetMethod
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
            public static void Vanilla(string call)
            {
                var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo? M() => typeof(C).GetMethod(nameof(this.ToString));

        public static int PublicStatic() => 0;

        public int PublicInstance() => 0;

        private static int PrivateStatic() => 0;

        private int PrivateInstance() => 0;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(this.ToString))", call);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Instance)")]
            [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
            [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static)")]
            [TestCase("typeof(string).GetMethod(\"MISSING\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
            public static void ExcludeNonPublicNotInSource(string invocation)
            {
                var code = @"
namespace N
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
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void ToStringOverridden()
            {
                var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString));
        }

        public override string? ToString() => base.ToString();
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void ToStringShadowing()
            {
                var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.ToString));
        }

        public new string? ToString() => base.ToString();
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void OverloadedMethodInSameType()
            {
                var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.M));
        }

        public void M()
        {
        }

        public int M(int i) => i;
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
            public static void ExplicitImplementation(string call)
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo? M() => typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void UnknownType()
            {
                var code = @"
namespace N
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
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void TypeParameter()
            {
                var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo? M<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("where T : C", "GetMethod(nameof(this.M1))")]
            [TestCase("where T : C", "GetMethod(nameof(this.M1), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
            [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
            public static void ConstrainedTypeParameter(string constraint, string call)
            {
                var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo? M1<T>()
            where T : C
        {
            return typeof(T).GetMethod(nameof(this.M1));
        }

        public int M1() => 0;

        public Type M1(Type type) => type;
    }
}".AssertReplace("where T : C", constraint)
      .AssertReplace("GetMethod(nameof(this.M1))", call);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void Generic()
            {
                var code = @"
namespace N
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
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("get_P")]
            [TestCase("set_P")]
            public static void PropertyAccessors(string name)
            {
                var code = @"
namespace N
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(""get_P"");
        }

        public int P { get; set; }
    }
}".AssertReplace("get_P", name);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("Delegate")]
            [TestCase("Action")]
            [TestCase("Action<int>")]
            public static void DelegateInvoke(string type)
            {
                var code = @"
namespace N
{
    using System;

    class C
    {
        public C(Delegate d)
        {
            var methodInfo = d.GetType().GetMethod(""Invoke"");
        }
    }
}".AssertReplace("Delegate", type);
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
