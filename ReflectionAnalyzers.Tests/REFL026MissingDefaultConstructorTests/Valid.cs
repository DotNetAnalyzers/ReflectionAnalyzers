namespace ReflectionAnalyzers.Tests.REFL026MissingDefaultConstructorTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly ActivatorAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL026NoDefaultConstructor;

    [TestCase("Activator.CreateInstance(typeof(C))")]
    [TestCase("Activator.CreateInstance(typeof(C), true)")]
    [TestCase("Activator.CreateInstance(typeof(C), false)")]
    [TestCase("Activator.CreateInstance(this.GetType())")]
    [TestCase("Activator.CreateInstance<C>()")]
    public static void ExplicitDefaultConstructor(string call)
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            var foo = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("(C?)Activator.CreateInstance(typeof(C))")]
    [TestCase("(C)Activator.CreateInstance(typeof(C))!")]
    [TestCase("Activator.CreateInstance<C>()")]
    public static void ImplicitDefaultConstructor(string call)
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public static C? Create() => Activator.CreateInstance<C>();
    }
}".AssertReplace("Activator.CreateInstance<C>()", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("Activator.CreateInstance(typeof(C), 1)")]
    [TestCase("Activator.CreateInstance(typeof(C), new object[] { 1 })")]
    public static void OneConstructorSingleIntParameter(string call)
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public C(int i)
        {
            var foo = Activator.CreateInstance(typeof(C));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C))", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("Activator.CreateInstance(typeof(C), \"abc\")")]
    [TestCase("Activator.CreateInstance(typeof(C), new object[] { null })")]
    [TestCase("Activator.CreateInstance(typeof(C), (string)null)")]
    public static void OneConstructorSingleStringParameter(string call)
    {
        var code = @"
#nullable disable
namespace N
{
    using System;

    public class C
    {
        public C(string text)
        {
            var foo = Activator.CreateInstance(typeof(C), ""abc"");
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(C), \"abc\")", call);

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void PrivateConstructor()
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        private C()
        {
            var foo = Activator.CreateInstance(typeof(C), true);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void WhenUnknown()
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? M(Type type) => Activator.CreateInstance(type, ""foo"");
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void WhenUnconstrainedGeneric()
    {
        var code = @"
namespace N
{
    using System;

    public class C
    {
        public static object? M<T>() => Activator.CreateInstance(typeof(T), ""foo"");
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void WhenOnInterfaceTypes()
    {
        var code = @"
namespace N
{
    using System;

    public interface A { }

    public class B : A
    {
        public B() { }
    }

    public class C
    {
        public C(A a)
        {
            var foo = (A?)Activator.CreateInstance(a.GetType());
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
}
