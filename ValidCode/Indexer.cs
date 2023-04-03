namespace ValidCode;

using System.Reflection;
using NUnit.Framework;

public class Indexer
{
    [Test]
    public void Valid()
    {
        Assert.NotNull(typeof(Indexer).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(typeof(Indexer).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, typeof(int), new[] { typeof(int) }, null));
    }

    public int this[int i] => 0;
}
