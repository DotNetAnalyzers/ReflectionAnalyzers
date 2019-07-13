namespace ReflectionAnalyzers.Tests.REFL029MissingTypesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new AddTypesFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL029MissingTypes.Descriptor);

        [Test]
        public static void GetMethodNoParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar));
        }

        public int Bar() => 0;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), Type.EmptyTypes);
        }

        public int Bar() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodNoParameterWithFlags()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int Bar() => 0;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        }

        public int Bar() => 0;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodOneParameter()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar));
        }

        public int Bar(int value) => value;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), new[] { typeof(int) });
        }

        public int Bar(int value) => value;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodOneParameterWithFlags()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int Bar(int value) => value;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }

        public int Bar(int value) => value;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodOneParams()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Linq;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar));
        }

        public int Bar(params int[] values) => values.Sum();
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Linq;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), new[] { typeof(int[]) });
        }

        public int Bar(params int[] values) => values.Sum();
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodOneParamsWithFlags()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Linq;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int Bar(params int[] values) => values.Sum();
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Linq;
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int[]) }, null);
        }

        public int Bar(params int[] values) => values.Sum();
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodTwoParameters()
        {
            var code = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar));
        }

        public double Bar(int i, double d) => i + d;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), new[] { typeof(int), typeof(double) });
        }

        public double Bar(int i, double d) => i + d;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void GetMethodTwoParameterWithFlags()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public double Bar(int i, double d) => i + d;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Reflection;

    class C
    {
        public C()
        {
            var methodInfo = typeof(C).GetMethod(nameof(this.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int), typeof(double) }, null);
        }

        public double Bar(int i, double d) => i + d;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
