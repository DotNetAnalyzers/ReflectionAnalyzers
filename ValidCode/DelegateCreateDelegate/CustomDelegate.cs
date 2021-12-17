// ReSharper disable All
namespace ValidCode.DelegateCreateDelegate
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public class CustomDelegate
    {
        delegate int StringInt(string text);
        delegate int EmptyInt();
        delegate void IntVoid(int n);
        delegate void Void();

        [Test]
        public void Valid()
        {
            Assert.AreEqual(3, ((StringInt)Delegate.CreateDelegate(
                                typeof(StringInt),
                                typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
                            .Invoke("abc"));

            Assert.AreEqual(3, ((StringInt?)Delegate.CreateDelegate(
                                typeof(StringInt),
                                typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!,
                                throwOnBindFailure: true))
                            ?.Invoke("abc"));

            Assert.AreEqual(3, ((EmptyInt)Delegate.CreateDelegate(
                                typeof(EmptyInt),
                                "abc",
                                typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
                            .Invoke());

            Assert.AreEqual(3, ((EmptyInt?)Delegate.CreateDelegate(
                                typeof(EmptyInt),
                                "abc",
                                typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!,
                                throwOnBindFailure: true))
                            ?.Invoke());

            ((Void)Delegate.CreateDelegate(
                    typeof(Void),
                    typeof(C).GetMethod(nameof(C.Void), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)!))
                .Invoke();

            ((IntVoid)Delegate.CreateDelegate(
                    typeof(IntVoid),
                    typeof(C).GetMethod(nameof(C.IntVoid), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)!))
                .Invoke(1);

            ((Void)Delegate.CreateDelegate(
                    typeof(Void),
                    "abc",
                    typeof(C).GetMethod(nameof(C.StringVoid), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
                .Invoke();
        }

        private static class C
        {
            public static void Void() { }

            public static void IntVoid(int _) { }

            public static void StringVoid(string _) { }

            public static int StringInt(string arg) => arg.Length;
        }
    }
}
