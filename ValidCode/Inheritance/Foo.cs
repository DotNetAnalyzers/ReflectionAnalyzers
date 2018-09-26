// ReSharper disable All
#pragma warning disable 67
#pragma warning disable 169
namespace ValidCode.Inheritance
{
    using System;
    using System.Reflection;

    public class FooBase
    {
        public static int PublicStaticField;

        private static readonly int PrivateStaticField;

        public static event EventHandler PublicStaticEvent;

        private static event EventHandler PrivateStaticEvent;

        public static int PublicStaticProperty { get; set; }

        private static int PrivateStaticProperty { get; set; }

        public static int PublicStaticMethod() => 0;

        private static int PrivateStaticMethod() => 0;
    }

    public class Foo : FooBase
    {
        public Foo()
        {
            typeof(Foo).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(Foo).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);

            typeof(FooBase).GetField(nameof(FooBase.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetField("PrivateStaticField", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetEvent(nameof(FooBase.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetEvent("PrivateStaticEvent", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetProperty(nameof(FooBase.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetProperty("PrivateStaticProperty", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(FooBase).GetMethod(nameof(FooBase.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(FooBase).GetMethod("PrivateStaticMethod", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        }
    }
}
