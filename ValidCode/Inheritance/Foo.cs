namespace ValidCode.Inheritance
{
    using System;
    using System.Reflection;

    public class FooBase
    {
        public static int PublicStaticField;

        public static event EventHandler PublicStaticEvent;

        public static int PublicStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;
    }

    public class Foo : FooBase
    {
        public Foo()
        {
            typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }
    }
}
