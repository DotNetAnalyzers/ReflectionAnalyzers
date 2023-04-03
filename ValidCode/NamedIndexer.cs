namespace ValidCode;

using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

public class NamedIndexer
{
    [Test]
    public void Valid()
    {
        Assert.NotNull(typeof(NamedIndexer).GetProperty("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [IndexerName("Bar")]
    public int this[byte i] => 0;
}
