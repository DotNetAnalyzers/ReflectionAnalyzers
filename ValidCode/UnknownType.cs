namespace ValidCode
{
    using System;
    using System.Reflection;

    public class UnknownType
    {
        public UnknownType(Type type)
        {
            _ = type.GetMethod("Foo");
            _ = type.GetMethod("Foo", new[] { typeof(int) });
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);

            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            _ = type.GetMethod("Foo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
        }
    }
}
