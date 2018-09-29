namespace ValidCode
{
    using System;

    public class Accessors
    {
        public Accessors()
        {
            _ = typeof(Foo).GetMethod("get_Bar");
            _ = typeof(Foo).GetMethod("set_Bar");
            _ = typeof(Foo).GetMethod("add_Baz");
            _ = typeof(Foo).GetMethod("remove_Baz");
        }

        public int Bar { get; set; }

        public event EventHandler Baz;
    }
}
