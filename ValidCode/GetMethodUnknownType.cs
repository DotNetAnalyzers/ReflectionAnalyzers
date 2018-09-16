namespace ValidCode
{
    using System;
    using System.Reflection;

    public class GetMethodUnknownType
    {
        public GetMethodUnknownType(Type unknownType)
        {
            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);

            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            unknownType.GetMethod("Bar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
        }
    }
}