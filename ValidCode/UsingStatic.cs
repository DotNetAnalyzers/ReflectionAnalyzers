namespace ValidCode
{
    using System;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class Foo
    {
        public Foo()
        {
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | Instance | DeclaredOnly, null, Type.EmptyTypes, null);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null);
            _ = typeof(Foo).GetMethod(nameof(this.Bar), Public | System.Reflection.BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null);
        }

        public int Bar() => 0;
    }
}
