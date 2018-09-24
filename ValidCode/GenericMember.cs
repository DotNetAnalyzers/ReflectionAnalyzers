namespace ValidCode
{
    using System.Reflection;

    public class GenericMember
    {
        public GenericMember()
        {
            _ = typeof(GenericMember).GetMethod(nameof(this.Id), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(GenericMember).GetNestedType("Bar`1", BindingFlags.Public);
        }

        public T Id<T>(T value) => value;

        public class Bar<T>
        {
        }
    }
}
