// ReSharper disable All
namespace ValidCode.DelegateCreateDelegate;

using System;
using System.Reflection;
using NUnit.Framework;

public class Instance
{
    [Test]
    public void Valid()
    {
        Assert.AreEqual(3, ((Func<C, string, int>)Delegate.CreateDelegate(
                            typeof(Func<C, string, int>),
                            typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
                        .Invoke(new C(), "abc"));

        Assert.AreEqual(3, ((Func<string, int>)Delegate.CreateDelegate(
                            typeof(Func<string, int>),
                            new C(),
                            typeof(C).GetMethod(nameof(C.StringInt), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
                        .Invoke("abc"));

        ((Action<C, int>)Delegate.CreateDelegate(
                typeof(Action<C, int>),
                typeof(C).GetMethod(nameof(C.IntVoid), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)!))
            .Invoke(new C(), 1);

        ((Action)Delegate.CreateDelegate(
                typeof(Action),
                new C(),
                typeof(C).GetMethod(nameof(C.Void), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)!))
            .Invoke();

        ((Action<int>)Delegate.CreateDelegate(
                typeof(Action<int>),
                new C(),
                typeof(C).GetMethod(nameof(C.IntVoid), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null)!))
            .Invoke(1);

        ((Action<string>)Delegate.CreateDelegate(
                typeof(Action<string>),
                new C(),
                typeof(C).GetMethod(nameof(C.StringVoid), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null)!))
            .Invoke("abc");

        Assert.AreEqual(1, ((Func<C, int>)Delegate.CreateDelegate(
                            typeof(Func<C, int>),
                            typeof(C).GetProperty(
                                nameof(C.Value),
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.GetMethod!)).Invoke(new C()));

        Assert.AreEqual(1, ((Func<C, int>)Delegate.CreateDelegate(
                            typeof(Func<C, int>),
                            typeof(C).GetProperty(
                                nameof(C.Value),
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly!)!.GetGetMethod()!)).Invoke(new C()));
    }

    private class C
    {
        public int Value { get; } = 1;

        public void Void() { }

        public void IntVoid(int _) { }

        public void StringVoid(string _) { }

        public int StringInt(string arg) => arg.Length;
    }
}
