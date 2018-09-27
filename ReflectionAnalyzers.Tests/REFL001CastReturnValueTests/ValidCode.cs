namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL001CastReturnValue.Descriptor);

        [TestCase("_ = ")]
        [TestCase("var _ = ")]
        [TestCase("var __ = ")]
        [TestCase("")]
        public void Discarding(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static void Bar()
        {
        }
    }
}".AssertReplace("_ = ", call);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void AssigningLocal()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void IsPattern()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            if (typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null) is string text)
            {
            }
        }

        public static string Bar() => null;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void SwitchPattern()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            switch (typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null))
            {
                case string text:
                    break;
            }
        }

        public static string Bar() => null;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void AssigningField()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly int value;

        public Foo()
        {
            this.value = (int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null);
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void UsingInExpression()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var text = ((int)typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null)).ToString();
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void CallingToString()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            var text = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null).ToString();
        }

        public static int Bar() => 0;
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("_ = ")]
        [TestCase("var _ = ")]
        [TestCase("var __ = ")]
        public void Discarded(string discard)
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(Bar)).Invoke(null, null).ToString();
        }

        public static int Bar() => 0;
    }
}".AssertReplace("_ = ", discard);

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
