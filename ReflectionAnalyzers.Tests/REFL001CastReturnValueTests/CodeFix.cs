namespace ReflectionAnalyzers.Tests.REFL001CastReturnValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ActivatorCreateInstance
        {
            private static readonly ActivatorAnalyzer Analyzer = new();
            private static readonly CastReturnValueFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL001CastReturnValue);

            [Test]
            public static void Typeof()
            {
                var before = @"
namespace N
{
    using System;

    public class C
    {
        public static void M()
        {
            var c = ↓Activator.CreateInstance(typeof(C));
        }
    }
}";

                var after = @"
namespace N
{
    using System;

    public class C
    {
        public static void M()
        {
            var c = (C)Activator.CreateInstance(typeof(C));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void WalkType()
            {
                var before = @"
namespace N
{
    using System;

    public class C
    {
        public static void M()
        {
            var type = typeof(C);
            var c = ↓Activator.CreateInstance(type);
        }
    }
}";

                var after = @"
namespace N
{
    using System;

    public class C
    {
        public static void M()
        {
            var type = typeof(C);
            var c = (C)Activator.CreateInstance(type);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [TestCase("Activator.CreateInstance(typeof(T))")]
            [TestCase("Activator.CreateInstance(typeof(T), true)")]
            [TestCase("Activator.CreateInstance(typeof(T), false)")]
            [TestCase("Activator.CreateInstance(typeof(T), \"foo\")")]
            public static void WhenUnconstrainedGeneric(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public static void M<T>()
        {
            var c = ↓Activator.CreateInstance(typeof(T));
        }
    }
}".AssertReplace("Activator.CreateInstance(typeof(T))", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
