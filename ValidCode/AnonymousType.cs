namespace ValidCode
{
    using System.Reflection;

    public class AnonymousType
    {
        public AnonymousType()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
