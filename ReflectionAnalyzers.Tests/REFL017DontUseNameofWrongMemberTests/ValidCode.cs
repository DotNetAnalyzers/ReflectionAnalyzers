namespace ReflectionAnalyzers.Tests.REFL017DontUseNameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL017DontUseNameofWrongMember.Descriptor;

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public static void GetNestedTypePrivateInOtherType(string type)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    public class C
    {
        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Bar
    {
        public Bar()
        {
            var member = typeof(C).GetNestedType(""Class"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("GetNestedType(\"Class\", BindingFlags.NonPublic)", $"GetNestedType(\"{type}\", BindingFlags.NonPublic)");

            RoslynAssert.Valid(Analyzer, Descriptor, fooCode, testCode);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitImplementationVisibleMembers(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    sealed class C : IDisposable
    {
        public C()
        {
            var method = typeof(C).GetMethod(nameof(IDisposable.Dispose));
        }

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
            var testCode = @"
using System;

namespace TestApp.Infrastructure
{
    static class Poke
    {
        public static void C(Delegate d) => d.GetType().GetMethod(nameof(Action.Invoke));
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void InterfaceMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    sealed class C : IDisposable
    {
        public C()
        {
            var method = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        }
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
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    class C
    {
        public object Get => typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void AnonymousTypeNameofInstanceProperty()
        {
            var testCode = @"
namespace RoslynSandbox
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
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public MethodInfo Bar<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void TypeOfConstrainedTGetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void TypeofDictionaryGetMethodAdd()
        {
            var testCode = @"
namespace RoslynSandbox
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
        public static void ThisGetTYpeGetStaticMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void ThisGetTYpeGetInstanceMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(this.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void WhenThrowingArgumentException()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void ArgumentOutOfRangeException()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoresDebuggerDisplay()
        {
            var testCode = @"
namespace RoslynSandbox
{
    [System.Diagnostics.DebuggerDisplay(""{Name}"")]
    public class C
    {
        public string Name { get; }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoresTypeName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoresSameLocal()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            var text = ""text"";
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenUsedInDeclaration()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenLocalsNotVisible()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoresNamespaceName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class C
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void AggregateExceptionInnerExceptionCount()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void NullableIntGetTypeGetFieldMaxValue()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public static object Get(int? value) => value.GetType().GetField(nameof(int.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
