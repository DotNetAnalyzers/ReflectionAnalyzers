// ReSharper disable All
namespace ValidCode;

using System;
using System.Reflection;

using NUnit.Framework;

public class Accessors
{
#pragma warning disable CS0067
    public event EventHandler? E;

    public int P { get; set; }

    [Test]
    public void Valid()
    {
        var instance = new Accessors { P = 1 };
#pragma warning disable REFL014
        Assert.NotNull(typeof(Accessors).GetMethod("get_P", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.AreEqual(1, typeof(Accessors).GetMethod("get_P", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.Invoke(instance, null));

        Assert.NotNull(typeof(Accessors).GetMethod("set_P", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Accessors).GetMethod("set_P", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.Invoke(instance, new object[] { 1 }));

        Assert.NotNull(typeof(Accessors).GetMethod("add_E", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Accessors).GetMethod("add_E", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.Invoke(instance, new object[] { new EventHandler((_, __) => { }) }));

        Assert.NotNull(typeof(Accessors).GetMethod("remove_E", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Accessors).GetMethod("remove_E", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.Invoke(instance, new object[] { new EventHandler((_, __) => { }) }));
#pragma warning restore REFL014
    }
}
