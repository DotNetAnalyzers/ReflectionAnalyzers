﻿namespace ReflectionAnalyzers.Tests.REFL030UseCorrectObjTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static partial class Valid
{
    public static class MethodInfoInvoke
    {
        private static readonly InvokeAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL030UseCorrectObj;

        [Test]
        public static void Static()
        {
            var code = @"
#pragma warning disable CS8602
namespace N
{
    public class C
    {
        public C()
        {
            _ = typeof(C).GetMethod(nameof(M)).Invoke(null, null);
        }

        public static void M()
        {
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Instance()
        {
            var code = @"
#pragma warning disable CS8602, CS8605
namespace N
{
    public class C
    {
        public C()
        {
            var value = (int)typeof(C).GetMethod(nameof(M)).Invoke(new C(), null);
        }

        public int M() => 0;
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [TestCase("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null)")]
        [TestCase("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke((int?)42, null)")]
        public static void Invoke(string call)
        {
            var code = @"
#pragma warning disable CS8602
namespace N
{
    using System;

    public static class C
    {
        public static object? M() => typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null);
    }
}".AssertReplace("typeof(int?).GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes).Invoke(42, null)", call);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
