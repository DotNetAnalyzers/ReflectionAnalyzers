﻿namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly DefaultMemberAttributeAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.REFL046DefaultMemberMustExist);

    /// <summary>
    /// Verify diagnostic is present when no such member exists.
    /// </summary>
    [Test]
    public static void DefaultMemberAbsent()
    {
        var code = @"
namespace N
{
    using System.Reflection;
    [DefaultMember(↓""NotValue"")]
    public class C
    {
        public int Value { get; set; }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    /// <summary>
    /// Verify events are not considered valid targets.
    /// </summary>
    [Test]
    public static void DefaultMemberIsEvent()
    {
        var code = @"
#pragma warning disable CS8618
namespace N
{
    using System;
    using System.Reflection;

    [DefaultMember(↓""E"")]
    public class C
    {
        public event EventHandler E;

        public void M() => this.E?.Invoke(this, EventArgs.Empty);
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    /// <summary>
    /// Verify base class names are not considered valid targets.
    /// </summary>
    [Test]
    public static void DefaultMemberIsBaseClass()
    {
        var code = @"
namespace N
{
    using System.Reflection;

    public class Base 
    {
        Base() 
        {
            System.Console.WriteLine(""Base constructor"");
        }
    }

    [DefaultMember(↓""Base"")]
    public class C { }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}
