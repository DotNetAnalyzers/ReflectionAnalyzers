namespace ReflectionAnalyzers.Tests.REFL013MemberIsOfWrongTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.REFL013MemberIsOfWrongType;

        [Test]
        public static void PassingArrayToMakeGenericType()
        {
            var code = @"
namespace N
{
    using System;

    class C<T1, T2>
    {
        void M(Type[] types)
        {
            typeof(C<,>).MakeGenericType(types);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Issue206WhenGettingField()
        {
            var code = @"
namespace ValidCode.Repros
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public delegate void OnModelHandler(object e);

    public interface IThing
    {
        event OnModelHandler OnModel;
    }

    public class Thing : IThing
    {
        public event OnModelHandler OnModel;
    }

    public class C
    {
        public static MulticastDelegate GetOnModel(IThing sender)
        {
            return (MulticastDelegate)sender.GetType()
                                            .GetField(""OnModel"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                            .GetValue(sender);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Issue206WhenGettingEvent()
        {
            var code = @"
namespace ValidCode.Repros
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public delegate void OnModelHandler(object e);

    public interface IThing
    {
        event OnModelHandler OnModel;
    }

    public class Thing : IThing
    {
        public event OnModelHandler OnModel;
    }

    public class C
    {
        public static EventInfo GetOnModel(IThing sender)
        {
            return sender.GetType()
                         .GetEvent(""OnModel"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
