namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL016UseNameof;

        [Test]
        public static void TypeofDictionaryGetMethodAdd()
        {
            var testCode = @"
namespace N
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void ThisGetTypeGetStaticMethod()
        {
            var testCode = @"
namespace N
{
    public class C
    {
        public object? Get() => this.GetType().GetMethod(nameof(Add));

        private static int Add(int x, int y) => x + y;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void ThisGetTypeGetInstanceMethod()
        {
            var testCode = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public MemberInfo? M() => this.GetType().GetMethod(nameof(this.Add));

        private int Add(int x, int y) => x + y;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("where T : C",               "GetMethod(nameof(this.M2))")]
        [TestCase("where T : C",               "GetMethod(nameof(this.M2), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : IConvertible",    "GetMethod(nameof(IConvertible.ToString))")]
        [TestCase("where T : IConvertible",    "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : IConvertible",    "GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("where T : IConvertible",    "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString))")]
        [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToString), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean))")]
        [TestCase("where T : C, IConvertible", "GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.Public | BindingFlags.Instance)")]
        public static void GetMethodWhenConstrainedTypeParameter(string constraint, string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MethodInfo? M1<T>(Type unused)
            where T : C
        {
            return typeof(T).GetMethod(nameof(this.M2));
        }

        public int M2() => 0;
    }
}".AssertReplace("where T : C", constraint)
  .AssertReplace("GetMethod(nameof(this.M2))", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"op_Addition\").Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Addition\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Equality\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Inequality\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })")]
        [TestCase("GetMethod(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { 1 })")]
        [TestCase("GetMethod(\"op_Explicit\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { (C)null })")]
        public static void Operators(string call)
        {
            var code = @"
#pragma warning disable CS8600, CS8602, CS8625
namespace N
{
    using System.Reflection;

    public class C
    {
        public object? Get(BindingFlags unused) => typeof(C).GetMethod(""op_Addition"", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null });

        public static C? operator +(C left, C right) => null;

        public static bool operator ==(C left, C right) => false;

        public static bool operator !=(C left, C right) => false;

        public static explicit operator int(C c) => 0;

        public static explicit operator C?(int c) => null;

        public override bool Equals(object? o) => false;

        public override int GetHashCode() => 0;
    }
}
".AssertReplace("GetMethod(\"op_Addition\", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, new object[] { null, null })", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Finalizer()
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
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetNestedType(\"Generic`1\", BindingFlags.Public)")]
        public static void GetNestedGenericType(string call)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetNestedType(""Generic`1"", BindingFlags.Public);
        }

        public class Generic<T>
        {
        }
    }
}".AssertReplace("GetNestedType(\"Generic`1\", BindingFlags.Public)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void NonPublicNotVisible()
        {
            var customAggregateException = @"
namespace N
{
    using System;

    public class CustomAggregateException : AggregateException
    {
        public int InnerExceptionCount { get; }
    }
}";
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var member = typeof(CustomAggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, customAggregateException, code);
        }

        [Test]
        //// ReSharper disable once InconsistentNaming
        public static void IEnumeratorGetCurrent()
        {
            var testCode = @"
namespace N
{
    using System.Collections;

    public class C
    {
        public void M(object value)
        {
            _ = typeof(IEnumerator).GetMethod(""get_Current"");
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("GetMethod(\"add_E\")")]
        [TestCase("GetMethod(\"remove_E\")")]
        public static void EventAccessors(string before)
        {
            var code = @"
#pragma warning disable CS8618
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public MemberInfo? Get() => typeof(C).GetMethod(""add_E"");

        public event EventHandler E;

        private void M() => E?.Invoke(null, EventArgs.Empty);
    }
}".AssertReplace("GetMethod(\"add_E\")", before);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(\"get_P\")")]
        [TestCase("GetMethod(\"set_P\")")]
        public static void PropertyAccessors(string before)
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MemberInfo? Get() => typeof(C).GetMethod(""get_P"");

        public int P { get; set; }
    }
}".AssertReplace("GetMethod(\"get_P\")", before);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WhenThrowingArgumentException()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ArgumentOutOfRangeException()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M(StringComparison value)
        {
            switch (value)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresDebuggerDisplay()
        {
            var code = @"
#pragma warning disable CS8618
namespace N
{
    [System.Diagnostics.DebuggerDisplay(""{Name}"")]
    public class C
    {
        public string Name { get; }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresTypeName()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M1()
        {
            this.M1(""Exception"");
        }

        public void M1(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresSameLocal()
        {
            var code = @"
namespace N
{
    public class C
    {
        public C()
        {
            var text = ""text"";
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, Settings.Default.WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS0219")));
        }

        [Test]
        public static void WhenUsedInDeclaration()
        {
            var code = @"
namespace N
{
    public class C
    {
        public C()
        {
            var text = Id(""text"");
        }

        private static string Id(string value) => value;
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenLocalsNotVisible()
        {
            var code = @"
namespace N
{
    public class C
    {
        public C()
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresNamespaceName()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M1()
        {
            this.M1(""Test"");
        }

        public void M1(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void AggregateExceptionInnerExceptionCount()
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = typeof(AggregateException).GetProperty(""InnerExceptionCount"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int InnerExceptionCount => 0;
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void GetMethodReferenceEquals()
        {
            var testCode = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public C()
        {
            var member = typeof(C).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("typeof(C).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(C).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)")]
        [TestCase("typeof(CBase).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetField(\"PrivateStaticField\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetEvent(nameof(CBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetEvent(\"PrivateStaticEvent\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetProperty(nameof(CBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetProperty(\"PrivateStaticProperty\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetMethod(nameof(CBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(CBase).GetMethod(\"PrivateStaticMethod\", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)")]
        public static void MemberInBase(string call)
        {
            var cBase = @"
#pragma warning disable CS8618
namespace N
{
    using System;

    public class CBase
    {
        public static int PublicStaticField;

        private static int PrivateStaticField = 1;

        public static event EventHandler PublicStaticEvent;

        private static event EventHandler PrivateStaticEvent;

        public static int PublicStaticProperty => PrivateStaticField;

        private static int PrivateStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;

        private static void PrivateStaticMethod()
        {
            PublicStaticEvent?.Invoke(null, EventArgs.Empty);
            PrivateStaticEvent?.Invoke(null, EventArgs.Empty);
        }
    }
}";
            var code = @"
namespace N
{
    using System.Reflection;

    public class C : CBase
    {
        public C()
        {
            typeof(C).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
    }
}".AssertReplace("typeof(C).GetField(nameof(CBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, cBase, code);
        }

        [Test]
        public static void SystemWindowsFormsControlCreateControl()
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public MemberInfo? Get => typeof(Control).GetMethod(""CreateControl"", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void SystemWindowsFormsControlCreateControlWhenSubclass()
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using System.Windows.Forms;

    class C : Control
    {
        public object? Get => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
