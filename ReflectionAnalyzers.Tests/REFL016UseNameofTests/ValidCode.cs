namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL016UseNameof.Descriptor;

        [Test]
        public void TypeofDictionaryGetMethodAdd()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void ThisGetTYpeGetStaticMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void ThisGetTypeGetInstanceMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(this.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
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
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"op_Addition\").Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Addition\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Equality\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Inequality\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { 1 })")]
        [TestCase("GetMethod(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { (Foo)null })")]
        public void Operators(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(""op_Addition"", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null });
        }

        public static Foo operator +(Foo left, Foo right) => null;

        public static bool operator ==(Foo left, Foo right) => false;

        public static bool operator !=(Foo left, Foo right) => false;

        public static explicit operator int(Foo c) => 0;

        public static explicit operator Foo(int c) => null;
    }
}
".AssertReplace("GetMethod(\"op_Addition\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })", call);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void Finalizer()
        {
            var code = @"
class C
{
    void M()
    {
        typeof(C).GetMethod(""Finalize"");
    }

    ~C()
    {
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetNestedType(\"Generic`1\", BindingFlags.Public)")]
        public void GetNestedGenericType(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(Foo).GetNestedType(""Generic`1"", BindingFlags.Public);
        }

        public class Generic<T>
        {
        }
    }
}".AssertReplace("GetNestedType(\"Generic`1\", BindingFlags.Public)", call);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void NonPublicNotVisible()
        {
            var exception = @"
namespace RoslynSandbox
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        public int InnerExceptionCount { get; }
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
            var member = typeof(CustomAggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, exception, code);
        }

        [Test]
        //// ReSharper disable once InconsistentNaming
        public void IEnumeratorGetCurrent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections;

    public class Foo
    {
        public void Meh(object value)
        {
            _ = typeof(IEnumerator).GetMethod(""get_Current"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("GetMethod(\"add_Public\")")]
        [TestCase("GetMethod(\"remove_Public\")")]
        public void EventAccessors(string before)
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
            var methodInfo = typeof(Foo).GetMethod(""add_Public"");
        }

        public event EventHandler Public;
    }
}".AssertReplace("GetMethod(\"add_Public\")", before);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"get_Public\")")]
        [TestCase("GetMethod(\"set_Public\")")]
        public void PropertyAccessors(string before)
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
            var methodInfo = typeof(Foo).GetMethod(""get_Public"");
        }

        public int Public { get; set; }
    }
}".AssertReplace("GetMethod(\"get_Public\")", before);

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
        public object Get => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void WhenThrowingArgumentException()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public void Meh(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ArgumentOutOfRangeException()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public void Meh(StringComparison value)
        {
            switch (value)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresDebuggerDisplay()
        {
            var testCode = @"
namespace RoslynSandbox
{
    [System.Diagnostics.DebuggerDisplay(""{Name}"")]
    public class Foo
    {
        public string Name { get; }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresTypeName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public void Bar()
        {
            this.Meh(""Exception"");
        }

        public void Meh(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresSameLocal()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            var text = ""text"";
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenUsedInDeclaration()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var text = Id(""text"");
        }

        private static string Id(string value) => value;
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenLocalsNotVisible()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            {
                var text = string.Empty;
            }

            {
                var text = Id(""text"");
            }

            {
                var text = string.Empty;
            }
        }

        private static string Id(string value) => value;
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresNamespaceName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public void Bar()
        {
            this.Meh(""Test"");
        }

        public void Meh(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AggregateExceptionInnerExceptionCount()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(AggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void GetMethodReferenceEquals()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Foo).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(Foo).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(FooBase).GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public void MemberInBase(string call)
        {
            var baseClass = @"
namespace RoslynSandbox
{
    using System;

    public class FooBase
    {
        public static int PublicStaticField;

        private static int PrivateStaticField;

        public static event EventHandler PublicStaticEvent;

        private static event EventHandler PrivateStaticEvent;

        public static int PublicStaticProperty { get; set; }

        private static int PrivateStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;

        private static int PrivateStaticMethod() => 0;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo : FooBase
    {
        public Foo()
        {
            typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, baseClass, code);
        }
    }
}
