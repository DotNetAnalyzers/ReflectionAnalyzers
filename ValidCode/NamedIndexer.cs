namespace ValidCode
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class NamedIndexer
    {
        public NamedIndexer()
        {
            _ = typeof(NamedIndexer).GetProperty("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        [IndexerName("Bar")]
        public int this[byte i] => 0;
    }
}
