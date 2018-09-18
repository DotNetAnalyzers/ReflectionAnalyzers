namespace ReflectionAnalyzers.Tests.REFL016UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new REFL016UseNameof();

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
    }
}
