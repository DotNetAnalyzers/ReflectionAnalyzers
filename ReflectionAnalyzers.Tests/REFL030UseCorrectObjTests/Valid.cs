namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ConstructorInfoInvoke
        {
            private static readonly DiagnosticAnalyzer Analyzer = new InvokeAnalyzer();
            private static readonly DiagnosticDescriptor Descriptor = REFL030UseCorrectObj.Descriptor;

            [TestCase("GetConstructor(Type.EmptyTypes).Invoke(null)")]
            [TestCase("GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 1 })")]
            public static void InvokeWithOneArgument(string call)
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
        }

        public C(int value)
        {
        }

        public static C Create() => (C)typeof(C).GetConstructor(Type.EmptyTypes).Invoke(null);
    }
}".AssertReplace("GetConstructor(Type.EmptyTypes).Invoke(null)", call);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [TestCase("type.GetConstructor(Type.EmptyTypes).Invoke(instance, null)")]
            [TestCase("type.GetConstructor(new[] { typeof(int) }).Invoke(instance, new object[] { 1 })")]
            public static void InvokeWithGetUninitializedObjectAndArgument(string call)
            {
                var code = @"
namespace N
{
    using System;
    using System.Runtime.Serialization;

    public class C
    {
        public C()
        {
        }

        public C(int value)
        {
        }

        public static void M()
        {
            var type = typeof(C);
            var instance = FormatterServices.GetUninitializedObject(type);
            type.GetConstructor(Type.EmptyTypes).Invoke(instance, null);
        }
    }
}".AssertReplace("type.GetConstructor(Type.EmptyTypes).Invoke(instance, null)", call);

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
