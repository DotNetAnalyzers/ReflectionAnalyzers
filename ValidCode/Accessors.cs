namespace ValidCode
{
    using System;
    using System.Reflection;

    public class Accessors
    {
        public Accessors()
        {
#pragma warning disable REFL014
            _ = typeof(Accessors).GetMethod("get_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(Accessors).GetMethod("set_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(Accessors).GetMethod("add_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(Accessors).GetMethod("remove_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#pragma warning restore REFL014
        }

        public int Bar { get; set; }

        public event EventHandler Baz;
    }
}
