namespace ReflectionAnalyzers.Tests.REFL028CastReturnValueToCorrectTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class ActivatorCreateInstance
    {
        private static readonly ActivatorAnalyzer Analyzer = new();
        private static readonly CastReturnValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL028CastReturnValueToCorrectType);

        [Test]
        public static void WhenCastingToWrongType()
        {
            var before = @"
#pragma warning disable CS8600
namespace N
{
    using System;

    public sealed class C : IDisposable
    {
        public C()
        {
            var foo = (↓string)Activator.CreateInstance(typeof(C));
        }

        public void Dispose()
        {
        }
    }
}";

            var after = @"
#pragma warning disable CS8600
namespace N
{
    using System;

    public sealed class C : IDisposable
    {
        public C()
        {
            var foo = (C)Activator.CreateInstance(typeof(C));
        }

        public void Dispose()
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
