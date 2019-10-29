namespace ReflectionAnalyzers.Tests.REFL031UseCorrectGenericArgumentsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class MakeGenericMethod
        {
            private static readonly DiagnosticAnalyzer Analyzer = new MakeGenericAnalyzer();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor);

            [Test]
            public static void CountError()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar<T>()
        {
            var method = typeof(C).GetMethod(nameof(C.Bar)).MakeGenericMethod↓(typeof(int), typeof(double));
        }
    }
}";
                var message = "The number of generic arguments provided doesn't equal the arity of the generic type definition. The member has 1 parameter but 2 arguments are passed in.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [Test]
            public static void ConstraintError()
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar<T>()
            where T : struct
        {
            var method = typeof(C).GetMethod(nameof(C.Bar)).MakeGenericMethod(↓typeof(string));
        }
    }
}";
                var message = "The argument typeof(string), on 'RoslynSandbox.C.Bar<T>()' violates the constraint of type 'T'.";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
            }

            [TestCase("MakeGenericMethod↓(new[] { typeof(int), typeof(double) })")]
            [TestCase("MakeGenericMethod(↓typeof(string))")]
            [TestCase("MakeGenericMethod(new[] { ↓typeof(string) })")]
            public static void ConstrainedParameterWrongArguments(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar<T>()
            where T : struct
        {
            var method = typeof(C).GetMethod(nameof(C.Bar)).MakeGenericMethod(↓typeof(string));
        }
    }
}".AssertReplace("MakeGenericMethod(↓typeof(string))", call);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("where T : class", "typeof(int)")]
            [TestCase("where T : struct", "typeof(string)")]
            [TestCase("where T : IComparable", "typeof(C)")]
            [TestCase("where T : new()", "typeof(Bar)")]
            public static void Constraints(string constraint, string arg)
            {
                var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public Bar(int i)
        {
        }
    }
}";
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public static void Bar<T>()
            where T : class
        {
            var method = typeof(C).GetMethod(nameof(C.Bar)).MakeGenericMethod(↓typeof(int));
        }
    }
}".AssertReplace("where T : class", constraint)
  .AssertReplace("typeof(int)", arg);
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, code);
            }

            [Test]
            public static void TernaryWrongOrder()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Reflection;

    public class C
    {
        public MethodInfo Get<T>()
        {
            return typeof(T).IsValueType
                ? typeof(C).GetMethod(nameof(this.ConstrainedToClass), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(↓typeof(T))
                : typeof(C).GetMethod(nameof(this.ConstrainedToStruct), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(↓typeof(T));
        }

        public T ConstrainedToClass<T>(T t)
            where T : class
        {
            return t;
        }

        public T ConstrainedToStruct<T>(T t)
            where T : struct
        {
            return t;
        }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
