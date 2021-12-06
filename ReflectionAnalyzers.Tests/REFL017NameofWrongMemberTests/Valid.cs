namespace ReflectionAnalyzers.Tests.REFL017NameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL017NameofWrongMember;

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public static void GetNestedTypePrivateInOtherType(string type)
        {
            var c = @"
namespace N
{
    public class C
    {
        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}";
            var c2 = @"
namespace N
{
    using System.Reflection;

    public class C2
    {
        public C2()
        {
            var member = typeof(C).GetNestedType(""Class"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("GetNestedType(\"Class\", BindingFlags.NonPublic)", $"GetNestedType(\"{type}\", BindingFlags.NonPublic)");

            RoslynAssert.Valid(Analyzer, Descriptor, c, c2);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitImplementationVisibleMembers(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    sealed class C : IDisposable
    {
        public MemberInfo Get() => typeof(C).GetMethod(nameof(IDisposable.Dispose));

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void DelegateInvoke()
        {
            var code = @"
using System;

namespace TestApp.Infrastructure
{
    static class Poke
    {
        public static void C(Delegate d) => d.GetType().GetMethod(nameof(Action.Invoke));
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void InterfaceMethod(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    sealed class C : IDisposable
    {
        public MemberInfo Get() => typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        public static void GetOverloadedMethod(string call)
        {
            var code = @"
namespace ValidCode
{
    using System.Reflection;

    public class OverloadedMethods
    {
        public OverloadedMethods()
        {
            typeof(OverloadedMethods).GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }
         public static int PublicStatic(int value) => value;
         public static double PublicStatic(double value) => value;
         public static int PublicStaticInstance(int value) => value;
         public double PublicStaticInstance(double value) => value;
         public int PublicInstance(int value) => value;
         public double PublicInstance(double value) => value;
         public int PublicPrivateInstance(int value) => value;
         private double PublicPrivateInstance(double value) => value;
    }
}".AssertReplace("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)", call);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
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
        public MemberInfo Get() => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void AnonymousTypeNameofInstanceProperty()
        {
            var testCode = @"
namespace N
{
    class C
    {
        public C()
        {
            var anon = new { C = 1 };
            var member = anon.GetType().GetProperty(nameof(anon.C));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void TypeOfTGetMethodGetHashCode()
        {
            var testCode = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo M<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void TypeOfConstrainedTGetMethod()
        {
            var testCode = @"
namespace N
{
    using System.Reflection;

    class C
    {
        public MethodInfo M1<T>()
            where T : C
        {
            return typeof(T).GetMethod(nameof(this.M1));
        }

        public int M1() => 0;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

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
        public object Get() => this.GetType().GetMethod(nameof(Add));

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
    public class C
    {
        public object Get() => this.GetType().GetMethod(nameof(this.Add));

        private int Add(int x, int y) => x + y;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
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
        public static void NullableIntGetTypeGetFieldMaxValue()
        {
            var code = @"
namespace N
{
    using System.Reflection;

    public class C
    {
        public static object Get(int? value) => value.GetType().GetField(nameof(int.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void GenericMethodOnGenericType()
        {
            var code = @"
namespace N
{
    using System.Reflection;

    class C<T1>
    {
        private void M<T2>()
        {
        }

        public object P => typeof(C<>).GetMethod(nameof(M), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
