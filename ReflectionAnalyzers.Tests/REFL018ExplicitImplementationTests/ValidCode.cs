namespace ReflectionAnalyzers.Tests.REFL018ExplicitImplementationTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL018ExplicitImplementation.Descriptor;

        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance)")]
        [TestCase("GetMethod(nameof(IConvertible.ToBoolean), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitImplementation(string call)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;
    class Foo
    {
        public Foo()
        {
            var methodInfo = typeof(string).GetMethod(nameof(IConvertible.ToBoolean));
        }
    }
}".AssertReplace("GetMethod(nameof(IConvertible.ToBoolean))", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(Foo).GetEvent(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        [TestCase("typeof(IFoo).GetEvent(nameof(IFoo.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)")]
        public void WhenExplicitAndExplicit(string call)
        {
            var interfaceCode = @"
namespace RoslynSandbox
{
    using System;

    public interface IFoo
    {
        event EventHandler Bar;
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Reflection;

    public sealed class Foo : IFoo
    {
        public Foo()
        {
            var member = typeof(Foo).GetEvent(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        internal event EventHandler Bar;

        event EventHandler IFoo.Bar
        {
            add => this.Bar += value;
            remove => this.Bar -= value;
        }
    }
}".AssertReplace("typeof(Foo).GetEvent(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)", call);

            AnalyzerAssert.Valid(Analyzer, Descriptor, interfaceCode, code);
        }
    }
}
