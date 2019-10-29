namespace ReflectionAnalyzers.Tests.REFL018ExplicitImplementationTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL018ExplicitImplementation.Descriptor;

        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitImplementation(string call)
        {
            var code = @"
namespace N
{
    using System;
    using System.Reflection;
    class C
    {
        public C()
        {
            var methodInfo = typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(C).GetEvent(nameof(this.E), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(IC).GetEvent(nameof(IC.E), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitAndExplicit(string call)
        {
            var interfaceCode = @"
namespace N
{
    using System;

    public interface IC
    {
        event EventHandler E;
    }
}";

            var code = @"
namespace N
{
    using System;
    using System.Reflection;

    public sealed class C : IC
    {
        public C()
        {
            var member = typeof(C).GetEvent(nameof(this.E), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        internal event EventHandler E;

        event EventHandler IC.E
        {
            add => this.E += value;
            remove => this.E -= value;
        }
    }
}".AssertReplace("typeof(C).GetEvent(nameof(this.E), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, interfaceCode, code);
        }
    }
}
