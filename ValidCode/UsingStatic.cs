namespace ValidCode
{
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | Static | DeclaredOnly);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | BindingFlags.Static | DeclaredOnly);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | System.Reflection.BindingFlags.Static | DeclaredOnly);
        }

        public int Bar() => 0;
    }
}
