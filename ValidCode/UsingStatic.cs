namespace ValidCode
{
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | Instance | DeclaredOnly);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | BindingFlags.Instance | DeclaredOnly);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | System.Reflection.BindingFlags.Instance | DeclaredOnly);
        }

        public int Bar() => 0;
    }
}
