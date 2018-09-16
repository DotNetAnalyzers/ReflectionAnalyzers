// ReSharper disable ReturnValueOfPureMethodIsNotUsed
namespace ValidCode
{
    using System;
    using System.Reflection;

    public class Base
    {
        protected static void ProtectedStatic() { }

        protected void ProtectedInstance() { }
    }

    public class GetMethod : Base
    {
        public GetMethod(Type unknownType)
        {
            typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(GetMethod).GetMethod(nameof(PublicPrivateStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(GetMethod).GetMethod(nameof(this.PublicInstanceMethod), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            typeof(GetMethod).GetMethod(nameof(PrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(GetMethod).GetMethod(nameof(this.PrivateInstanceMethod), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            typeof(GetMethod).GetMethod(nameof(this.PublicPrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            typeof(GetMethod).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(GetMethod).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance);
            typeof(GetMethod).GetMethod(nameof(ProtectedStatic), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            typeof(GetMethod).GetMethod(nameof(this.ProtectedInstance), BindingFlags.NonPublic | BindingFlags.Instance);


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

        public static int PublicStaticMethod() => 0;

        public static int PublicPrivateStaticMethod() => 0;

        public int PublicInstanceMethod() => 0;

        private static int PrivateStaticMethod() => 0;

        private int PrivateInstanceMethod() => 0;

        private static int PublicPrivateStaticMethod(int i) => i;
    }
}
