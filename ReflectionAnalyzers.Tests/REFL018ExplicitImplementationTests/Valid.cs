namespace ReflectionAnalyzers.Tests.REFL018ExplicitImplementationTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly GetXAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL018ExplicitImplementation;

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
        [TestCase("typeof(I).GetEvent(nameof(I.E), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public static void WhenExplicitAndExplicit(string call)
        {
            var iC = @"
namespace N
{
    using System;

    public interface I
    {
        event EventHandler E;
    }
}";

            var code = @"
#pragma warning disable CS8618
namespace N
{
    using System;
    using System.Reflection;

    public sealed class C : I
    {
        public MemberInfo? Get() => typeof(C).GetEvent(nameof(this.E), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        internal event EventHandler E;

        event EventHandler I.E
        {
            add => this.E += value;
            remove => this.E -= value;
        }

        public void M() => this.E?.Invoke(this, EventArgs.Empty);
    }
}".AssertReplace("typeof(C).GetEvent(nameof(this.E), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, iC, code);
        }
    }
}
