namespace ValidCode
{
    using System.Reflection;

    public class GetNestedType
    {
        public GetNestedType()
        {
            typeof(GetNestedType).GetNestedType(nameof(PublicStatic),  BindingFlags.Public | BindingFlags.DeclaredOnly);
            typeof(GetNestedType).GetNestedType(nameof(Public),        BindingFlags.Public | BindingFlags.DeclaredOnly);
            typeof(GetNestedType).GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            typeof(GetNestedType).GetNestedType(nameof(Private),       BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        }

        public static class PublicStatic
        {
        }

        public class Public
        {
        }

        private static class PrivateStatic
        {
        }

        private class Private
        {
        }
    }
}
