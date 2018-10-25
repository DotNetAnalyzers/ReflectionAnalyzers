namespace ReflectionAnalyzers.Tests.REFL017DontUseNameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL017DontUseNameofWrongMember.Descriptor;

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public void GetNestedTypePrivateInOtherType(string type)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
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
            var member = typeof(Foo).GetNestedType(""Class"", BindingFlags.NonPublic);
        }
    }
}".AssertReplace("GetNestedType(\"Class\", BindingFlags.NonPublic)", $"GetNestedType(\"{type}\", BindingFlags.NonPublic)");

            AnalyzerAssert.Valid(Analyzer, Descriptor, fooCode, testCode);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitImplementationVisibleMembers(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    sealed class Foo : IDisposable
    {
        public Foo()
        {
            var method = typeof(Foo).GetMethod(nameof(IDisposable.Dispose));
        }

        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public void DelegateInvoke()
        {
            var testCode = @"
using System;

namespace TestApp.Infrastructure
{
    static class Poke
    {
        public static void Foo(Delegate d) => d.GetType().GetMethod(nameof(Action.Invoke));
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [TestCase("GetMethod(nameof(IDisposable.Dispose))")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void InterfaceMethod(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    sealed class Foo : IDisposable
    {
        public Foo()
        {
            var method = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        }
        void IDisposable.Dispose()
        {
        }
    }
}".AssertReplace("GetMethod(nameof(IDisposable.Dispose))", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)")]
        [TestCase("GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(double) }, null)")]
        public void GetOverloadedMethod(string call)
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
        public void AnonymousTypeNameofInstanceProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    class Foo
    {
        public Foo()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty(nameof(anon.Foo));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void TypeOfTGetMethodGetHashCode()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class Foo
    {
        public MethodInfo Bar<T>() => typeof(T).GetMethod(nameof(this.GetHashCode));
    }
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void TypeOfConstrainedTGetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
}";
            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

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
        public void ThisGetTYpeGetInstanceMethod()
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
        public void NullableIntGetTypeGetFieldMaxValue()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public static object Get(int? value) => value.GetType().GetField(nameof(int.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
