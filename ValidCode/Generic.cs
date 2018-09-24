namespace ValidCode
{
    using System.Reflection;

    public class Generic
    {
        public MethodInfo Bar<T>() => typeof(T).GetMethod(nameof(this.GetHashCode), BindingFlags.Public | BindingFlags.Instance);
    }
}
