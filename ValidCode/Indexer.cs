namespace ValidCode
{
    using System.Reflection;

    public class Indexer
    {
        public Indexer()
        {
            _ = typeof(Indexer).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public int this[int p1] => 0;
    }
}
