﻿namespace ReflectionAnalyzers.Tests.Helpers.Reflection
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static partial class GetXTests
    {
        public static class GetMethodOverloadResolution
        {
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), new[] { typeof(System.Type), typeof(int) })", "System.Array.CreateInstance(System.Type, int)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", "System.Array.CreateInstance(System.Type, int)")]
            [TestCase("typeof(System.Activator).GetMethod(nameof(System.Activator.CreateInstance), new[] { typeof(System.Reflection.TypeInfo) })", "System.Activator.CreateInstance(System.Type)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int32) })", "System.Array.GetValue(int)")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.Int64) })", "System.Array.GetValue(long)")]
            public static void Success(string call, string expected)
            {
                var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class C
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) });
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(int) })", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, semanticModel, CancellationToken.None, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected, reflectedMember.Symbol.ToDisplayString());
            }

            [TestCase("typeof(Array).GetMethod(nameof(Array.CreateInstance), new[] { typeof(Type), typeof(string) })")]
            [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(object) })")]
            [TestCase("typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(MemberInfo) })")]
            [TestCase("typeof(System.Array).GetMethod(nameof(System.Array.GetValue), new[] { typeof(System.IFormattable) })")]
            public static void NoMatch(string call)
            {
                var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class C
    {
        public object Get() => typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null);
    }
}".AssertReplace("typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null)", call);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, semanticModel, CancellationToken.None, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.WrongTypes, reflectedMember.Match);
            }

            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable) })", "N.C.M(System.IComparable)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IConvertible) })", "N.C.M(System.IConvertible)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IEquatable<int>) })", "N.C.M(System.IEquatable<int>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IEquatable<double>) })", "N.C.M(System.IEquatable<double>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable<int>) })", "N.C.M(System.IComparable<int>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IComparable<double>) })", "N.C.M(System.IComparable<double>)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(int) })", "N.C.M(int)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(double) })", "N.C.M(double)")]
            [TestCase("typeof(C).GetMethod(nameof(M), new[] { typeof(IFormattable) })", "N.C.M(System.IFormattable)")]
            public static void SuccessWhenInSameType(string call, string expected)
            {
                var code = @"
namespace N
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
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, semanticModel, CancellationToken.None, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected, reflectedMember.Symbol.ToDisplayString());
            }

            [TestCase("typeof(A).GetMethod(\"M\", new[] { typeof(int) })", "N.A.M(int)")]
            [TestCase("typeof(B).GetMethod(\"M\", new[] { typeof(int) })", "N.A.M(int)")]
            [TestCase("typeof(B).GetMethod(\"M\", new[] { typeof(double) })", "N.B.M(double)")]
            public static void SuccessWhenInheritance(string call, string expected)
            {
                var code = @"
namespace N
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
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("GetMethod");
                Assert.AreEqual(true, GetX.TryMatchGetMethod(invocation, semanticModel, CancellationToken.None, out var reflectedMember, out _, out _, out _));
                Assert.AreEqual(FilterMatch.Single, reflectedMember.Match);
                Assert.AreEqual(expected, reflectedMember.Symbol.ToDisplayString());
            }
        }
    }
}
