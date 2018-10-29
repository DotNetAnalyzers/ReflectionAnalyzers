namespace ValidCode
{
    using System;

    public class CreateDelegate
    {
        public static int M(string arg) => arg.Length;

        public static Func<string, int> Get => (Func<string, int>)Delegate.CreateDelegate(
            typeof(Func<string, int>),
            typeof(CreateDelegate).GetMethod(nameof(M)));
    }
}
