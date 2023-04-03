namespace ValidCode;

using System;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;

public class CustomAggregateException : AggregateException
{
    private int InnerExceptionCount { get; }
}

class NotInSource
{
    [Test]
    public void Valid()
    {
        const string InnerExceptionCount = "InnerExceptionCount";
        Assert.NotNull(typeof(AggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.NotNull(typeof(AggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(typeof(CustomAggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        Assert.NotNull(typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null));
        Assert.NotNull(typeof(Control).GetMethod(nameof(Control.CreateControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null));
    }
}
