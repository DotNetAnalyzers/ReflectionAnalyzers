namespace ReflectionAnalyzers.Tests.REFL046DefaultMemberMustExistTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new DefaultMemberAttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = REFL046DefaultMemberMustExist.Descriptor;

        /// <summary>
        /// Verify properties are considered valid targets.
        /// </summary>
        [Test]
        public void DefaultMemberPresentAsProperty()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class Foo
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
        public void DefaultMemberPresentAsField()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class Foo
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
        public void DefaultMemberPresentAsInstanceMethod()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class Foo
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
        public void DefaultMemberPresentAsStaticMethod()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Value"")]
public class Foo
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
        public void DefaultMemberPresentAsConstructor()
        {
            var code = @"
using System.Reflection;
[DefaultMember(""Foo"")]
public class Foo
{
    public int Value { get; }

    public Foo(int value)
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
        public void DefaultMemberPresentInParent()
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
