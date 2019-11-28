namespace ReflectionAnalyzers.Tests.REFL033UseSameTypeAsParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly CodeFixProvider Fix = new UseParameterTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL033UseSameTypeAsParameter);

        [TestCase("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)")]
        [TestCase("typeof(C).GetMethod(nameof(this.Public), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)")]
        public static void GetMethodOneParameterOverloadResolution(string call)
        {
            var before = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public object Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null)", call);

            var after = @"
namespace N
{
    using System;
    using System.Reflection;

    class C
    {
        public object Get() => typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);

        public static IComparable Static(IComparable i) => i;

        public IComparable Public(IComparable i) => i;

        private IComparable Private(IComparable i) => i;
    }
}".AssertReplace("typeof(C).GetMethod(nameof(Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null)", call.AssertReplace("↓int", "IComparable"));

            var message = "Use the same type as the parameter. Expected: IComparable.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after);
        }

        [Test]
        public static void TwoProjects()
        {
            var c1 = @"
namespace Project1
{
    using System;

    public class C1
    {
        public static IComparable Static(IComparable i) => i;
    }
}";

            var before = @"
namespace Project2
{
    using System;
    using System.Reflection;

    using Project1;

    public class C2
    {
        public object Get() => typeof(C1).GetMethod(nameof(C1.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(↓int) }, null);
    }
}";

            var after = @"
namespace Project2
{
    using System;
    using System.Reflection;

    using Project1;

    public class C2
    {
        public object Get() => typeof(C1).GetMethod(nameof(C1.Static), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(IComparable) }, null);
    }
}";
            var message = "Use the same type as the parameter. Expected: IComparable.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), new[] { c1, before }, after);
        }

        [Test]
        public static void Issue121Inline()
        {
            var before = @"
namespace N
{
    using System;
    using System.Collections.Generic;

    class C
    {
        public object Get => typeof(C).GetMethod(nameof(M), new[] { typeof(Type), typeof(↓Dictionary<string, object>) });

        public void M(Type type, IReadOnlyDictionary<string, object> parameters)
        {
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.Collections.Generic;

    class C
    {
        public object Get => typeof(C).GetMethod(nameof(M), new[] { typeof(Type), typeof(IReadOnlyDictionary<string, object>) });

        public void M(Type type, IReadOnlyDictionary<string, object> parameters)
        {
        }
    }
}";

            var message = "Use the same type as the parameter. Expected: IReadOnlyDictionary<string, object>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after, fixTitle: "Change to: IReadOnlyDictionary<string, object>.");
        }

        [Test]
        public static void Issue121()
        {
            var before = @"
namespace N
{
    using System;
    using System.Collections.Generic;

    class C
    {
        public object Get
        {
            get
            {
                var dictionaryType = typeof(Dictionary<string, object>);
                return typeof(C).GetMethod(nameof(this.M), new[] { typeof(Type), ↓dictionaryType });
            }
        }

        public void M(Type type, IReadOnlyDictionary<string, object> parameters)
        {
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.Collections.Generic;

    class C
    {
        public object Get
        {
            get
            {
                var dictionaryType = typeof(Dictionary<string, object>);
                return typeof(C).GetMethod(nameof(this.M), new[] { typeof(Type), typeof(IReadOnlyDictionary<string, object>) });
            }
        }

        public void M(Type type, IReadOnlyDictionary<string, object> parameters)
        {
        }
    }
}";

            var message = "Use the same type as the parameter. Expected: IReadOnlyDictionary<string, object>.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after, fixTitle: "Change to: typeof(IReadOnlyDictionary<string, object>).");
        }
    }
}
