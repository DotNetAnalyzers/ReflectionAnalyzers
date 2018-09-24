namespace ValidCode
{
    using System.Reflection;

    public class GenericMember
    {
        public GenericMember()
        {
            _ = typeof(GenericMember).GetMethod(nameof(GenericMember.Id), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public T Id<T>(T value) => value;
    }
}
