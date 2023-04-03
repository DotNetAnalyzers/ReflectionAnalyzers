// ReSharper disable All
namespace ValidCode;

using System;
using System.Reflection;
using NUnit.Framework;

public class GetConstructor
{
    [Test]
    public void Valid()
    {
        Assert.NotNull(typeof(Default).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
        Assert.NotNull(typeof(Single).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
        Assert.NotNull(typeof(Two).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null));
        Assert.NotNull(typeof(Two).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null));
    }

    public class Default
    {
    }

    public class Single
    {
        public Single()
        {
        }
    }

    public class Two
    {
        public Two(int value)
        {
        }

        public Two(double value)
        {
        }
    }
}
