namespace ValidCode
{
    using System;
    using System.Reflection;

    public class UnknownType
    {
        public UnknownType(Type type)
        {
            type.GetMethod("Foo");
            type.GetMethod("Foo", new[] { typeof(int) });
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);

            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
        }
    }
}
