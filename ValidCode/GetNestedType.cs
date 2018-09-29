namespace ValidCode
{
    using System.Reflection;

    public class GetNestedType
    {
        public GetNestedType()
        {
            _ = typeof(GetNestedType).GetNestedType(nameof(PublicStatic), BindingFlags.Public);
            _ = typeof(GetNestedType).GetNestedType(nameof(Public), BindingFlags.Public);
            _ = typeof(GetNestedType).GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic);
            _ = typeof(GetNestedType).GetNestedType(nameof(Private), BindingFlags.NonPublic);
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
