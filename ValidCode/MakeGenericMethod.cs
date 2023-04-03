namespace ValidCode;

using System;
using System.Reflection;
using NUnit.Framework;

public class MakeGenericMethod
{
    [Test]
    public void Valid()
    {
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(int)));
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(M), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(string)));
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(ConstrainedToIComparableOfT), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(int)));
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(ConstrainedToIComparableOfT), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(string)));
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(ConstrainedToClass), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(string)));
        Assert.NotNull(typeof(MakeGenericMethod).GetMethod(nameof(ConstrainedToStruct), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(int)));
    }

    public T M<T>(T t) => t;

    public MethodInfo Get<T>()
    {
        return typeof(T).IsValueType
            ? typeof(MakeGenericMethod).GetMethod(nameof(this.ConstrainedToStruct), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(int))
            : typeof(MakeGenericMethod).GetMethod(nameof(this.ConstrainedToClass), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.MakeGenericMethod(typeof(string));
    }

    public T ConstrainedToIComparableOfT<T>(T t)
        where T : IComparable<T>
    {
        return t;
    }

    public T ConstrainedToClass<T>(T t)
        where T : class
    {
         return t;
    }

    public T ConstrainedToStruct<T>(T t)
        where T : struct
    {
        return t;
    }
}
