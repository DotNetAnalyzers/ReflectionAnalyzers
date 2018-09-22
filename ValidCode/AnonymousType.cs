namespace ValidCode
{
    using System.Reflection;

    public class AnonymousType
    {
        public AnonymousType()
        {
            var anon = new { Foo = 1 };
            var member = anon.GetType().GetProperty(nameof(anon.Foo), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
