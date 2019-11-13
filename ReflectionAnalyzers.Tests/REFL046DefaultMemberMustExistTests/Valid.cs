namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL046DefaultMemberMustExist.Descriptor;

        /// <summary>
        /// Verify properties are considered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentAsProperty()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class C
{
    public int Value { get; set; }
}
";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        /// <summary>
        /// Verify fields are considered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentAsField()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class C
{
    public int Value;
}
";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        /// <summary>
        /// Verify instance methods are condidered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentAsInstanceMethod()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class C
{
    public int Value() => 42;
}
";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        /// <summary>
        /// Verify static methods are condidered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentAsStaticMethod()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class C
{
    public static int Value() => 42;
}
";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        /// <summary>
        /// Verify constructors are considered valid targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentAsConstructor()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""C"")]
public class C
{
    public int Value { get; }

    public C(int value)
    {
        Value = value;
    }
}
";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        /// <summary>
        /// Verify base classes are checked for targets.
        /// </summary>
        [Test]
        public static void DefaultMemberPresentInParent()
        {
            var code = @"
using System.Reflection;

public class Base
{
    public int Value { get; set; }
}

[DefaultMember(""Value"")]
public class Derived : Base { }

";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
