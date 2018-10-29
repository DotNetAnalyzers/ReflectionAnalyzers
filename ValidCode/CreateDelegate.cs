namespace ValidCode
{
    using System;
    using System.Reflection;

    public class CreateDelegate
    {
        public static int M(string arg) => arg.Length;

        public static Func<string, int> Get => (Func<string, int>)Delegate.CreateDelegate(
            typeof(Func<string, int>),
            typeof(CreateDelegate).GetMethod(nameof(M), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));
    }
}
