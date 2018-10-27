namespace ReflectionAnalyzers.Tests.REFL041CreateDelegateTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CreateDelegateAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL041CreateDelegateType.Descriptor;

        [Test]
        public void CreateDelegate()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    class C
    {
        public static object Get => Delegate.CreateDelegate(
            typeof(Func<Type, string, bool, ParameterExpression>),
            typeof(ParameterExpression).GetMethod(""Make"", BindingFlags.Static | BindingFlags.NonPublic));
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}