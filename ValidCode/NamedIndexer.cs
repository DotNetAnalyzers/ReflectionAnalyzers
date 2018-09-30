namespace ValidCode
{
    using NUnit.Framework;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
}
