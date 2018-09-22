namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using ReflectionAnalyzers.Codefixes;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new NameofAnalyzer();
        private static readonly CodeFixProvider Fix = new NameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL016UseNameof.DiagnosticId);

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
            var member = typeof(Dictionary<string, object>).GetMethod(↓""Add"");
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenNotUsingNameofTarget()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(↓Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenUsingNameofOther()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenUsingNameofFromOtherTypeStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenUsingNameofFromOtherTypeInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenUsingNameofFromOtherTypeInstanceUnderscore()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = GetType().GetMethod(nameof(HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        public Foo()
        {
            var member = GetType().GetMethod(nameof(Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public void GetNestedTypePrivateInSameType(string type)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetNestedType(↓""Class"", BindingFlags.NonPublic);
        }

        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}".AssertReplace("GetNestedType(↓\"Class\", BindingFlags.NonPublic)", $"GetNestedType(↓\"{type}\", BindingFlags.NonPublic)");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public Foo()
        {
            var member = this.GetType().GetNestedType(nameof(Class), BindingFlags.NonPublic);
        }

        private class Class { }

        private enum Enum { }

        private interface Interface { }

        private struct Struct { }
    }
}".AssertReplace("nameof(Class)", $"nameof({type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("Class")]
        [TestCase("Enum")]
        [TestCase("Interface")]
        [TestCase("Struct")]
        public void GetNestedTypePublicInOtherType(string type)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Foo
    {
        public class Class { }

        public enum Enum { }

        public interface Interface { }

        public struct Struct { }
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
            var member = typeof(Foo).GetNestedType(↓""Class"", BindingFlags.Public);
        }
    }
}".AssertReplace("GetNestedType(↓\"Class\", BindingFlags.Public)", $"GetNestedType(↓\"{type}\", BindingFlags.Public)");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class Bar
    {
        public Bar()
        {
            var member = typeof(Foo).GetNestedType(nameof(Foo.Class), BindingFlags.Public);
        }
    }
}".AssertReplace("nameof(Foo.Class)", $"nameof(Foo.{type})");

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooCode, testCode }, fixedCode);
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
                throw new ArgumentNullException(↓""value"");
            }
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenThrowingArgumentOutOfRangeException()
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
                    throw new ArgumentOutOfRangeException(↓""value"", value, null);
            }
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenRaisingPropertyChanged()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value*this.Value;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(↓""Squared"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value*this.Value;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Squared));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenRaisingStaticPropertyChanged()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public static class Foo
    {
        private static string name;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""Name""));
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public static class Foo
    {
        private static string name;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenRaisingStaticPropertyChanged2()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class Foo
    {
        private static string name;
        private int value;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""Name""));
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class Foo
    {
        private static string name;
        private int value;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenStaticNameofInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Value { get; set; }

        public static void Bar()
        {
            Bar(↓""Value"");
        }

        public static void Bar(string meh)
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Value { get; set; }

        public static void Bar()
        {
            Bar(nameof(Value));
        }

        public static void Bar(string meh)
        {
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenStaticNameofInstance2()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static readonly string Name = Bar(↓""Value"");

        public int Value { get; set; }

        public static string Bar(string meh) => meh;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static readonly string Name = Bar(nameof(Value));

        public int Value { get; set; }

        public static string Bar(string meh) => meh;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenStaticNameofInstance3()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static readonly string Name = string.Format(↓""Value"");

        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static readonly string Name = string.Format(nameof(Value));

        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenRaisingPropertyChangedUnderscoreNames()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => _value*_value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(↓""Squared"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => _value*_value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Squared));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
