namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public partial class GetXTests
    {
        public class GetMethodOverloadResolution
        {
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), new[] { typeof(System.Type), typeof(int) })",                                                                             "System.Array.CreateInstance(System.Type, int)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", "System.Array.CreateInstance(System.Type, int)")]
            [TestCase("typeof(System.Activator).GetMethod(nameof(System.Activator.CreateInstance), new[] { typeof(System.Reflection.TypeInfo) })",                                                                   "System.Activator.CreateInstance(System.Type)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int32) })",                                                                                               "System.Array.GetValue(int)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int64) })",                                                                                               "System.Array.GetValue(long)")]
            public void Success(string call, string expected)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Foo
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) });
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) })", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] {syntaxTree}, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
                Assert.AreEqual(true,               GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected,           reflectedMember.Symbol.ToDisplayString());
            }

            [TestCase("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(string) })")]
            [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(object) })")]
            [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(MemberInfo) })")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.IFormattable) })")]
            public void NoMatch(string call)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Foo
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null);
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] {syntaxTree}, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
                Assert.AreEqual(true,                   GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.WrongTypes, reflectedMember.Match);
            }

            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable) })",         "RoslynSandbox.C.M(System.IComparable)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IConvertible) })",        "RoslynSandbox.C.M(System.IConvertible)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IEquatable<int>) })",     "RoslynSandbox.C.M(System.IEquatable<int>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IEquatable<double>) })",  "RoslynSandbox.C.M(System.IEquatable<double>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable<int>) })",    "RoslynSandbox.C.M(System.IComparable<int>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable<double>) })", "RoslynSandbox.C.M(System.IComparable<double>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(int) })",                 "RoslynSandbox.C.M(int)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(double) })",              "RoslynSandbox.C.M(double)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IFormattable) })",        "RoslynSandbox.C.M(System.IFormattable)")]
            public void SuccessWhenInSameType(string call, string expected)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class C
    {
        public object Get => typeof(C).GetMethod(nameof(M), new[] { typeof(int) });

        public static void M(IComparable _) { }

        public static void M(IConvertible _) { }
       
        public static void M(IEquatable<int> _) { }
        
        public static void M(IEquatable<double> _) { }

        public static void M(IComparable<int> _) { }

        public static void M(IComparable<double> _) { }

        public static void M(int _) { }

        public static void M(double _) { }
 
        public static void M(IFormattable _) { }
    }
}".AssertReplace("typeof(C).GetMethod(nameof(M), new[] { typeof(int) })", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] {syntaxTree}, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
                Assert.AreEqual(true,               GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected,           reflectedMember.Symbol.ToDisplayString());
            }

            [TestCase("typeof(A).GetMethod(\"M\", new[] { typeof(int) })",    "RoslynSandbox.A.M(int)")]
            [TestCase("typeof(B).GetMethod(\"M\", new[] { typeof(int) })",    "RoslynSandbox.A.M(int)")]
            [TestCase("typeof(B).GetMethod(\"M\", new[] { typeof(double) })", "RoslynSandbox.B.M(double)")]
            public void SuccessWhenInheritance(string call, string expected)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;

    public class A
    {
        public void M(int i) { }
    }

    public class B : A
    {
        public void M(double i) { }
    }

    public class C
    {
        public object Get => typeof(A).GetMethod(""M"", new[] { typeof(int) });
    }
}".AssertReplace("typeof(A).GetMethod(\"M\", new[] { typeof(int) })", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] {syntaxTree}, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                var context = new SyntaxNodeAnalysisContext(invocation, null, semanticModel, null, null, null, CancellationToken.None);
                Assert.AreEqual(true,               GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected,           reflectedMember.Symbol.ToDisplayString());
            }
        }
    }
}
