// ReSharper disable All
namespace ValidCode;

using System;
using System.Reflection;
using NUnit.Framework;
using static System.Reflection.BindingFlags;

class UsingStatic
{
    [Test]
    public void Valid()
    {
        Assert.NotNull(typeof(UsingStatic).GetMethod(nameof(this.Bar), Public | Instance | DeclaredOnly, null, Type.EmptyTypes, null));
        Assert.NotNull(typeof(UsingStatic).GetMethod(nameof(this.Bar), Public | BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null));
        Assert.NotNull(typeof(UsingStatic).GetMethod(nameof(this.Bar), Public | System.Reflection.BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null));
    }

    public int Bar() => 0;
}
