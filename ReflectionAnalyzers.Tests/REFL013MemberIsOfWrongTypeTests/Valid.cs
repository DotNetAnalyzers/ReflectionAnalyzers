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
namespace N
{
    using System;
    using System.Reflection;

    public delegate void EHandler(object e);

    public interface I
    {
        event EHandler E;
    }

    public class C : I
    {
        public event EHandler E;

        private void OnE() => this.E?.Invoke(null);
    }

    public class C1
    {
        public static MulticastDelegate GetOnModel(I sender)
        {
            return (MulticastDelegate)sender.GetType()
                                            .GetField(""E"", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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
namespace N
{
    using System.Reflection;

    public delegate void EHandler(object e);

    public interface I
    {
        event EHandler E;
    }

    public class C : I
    {
        public event EHandler E;

        private void OnE() => this.E?.Invoke(null);
    }

    public class C1
    {
        public static EventInfo GetOnModel(I sender)
        {
            return sender.GetType()
                         .GetEvent(""E"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
