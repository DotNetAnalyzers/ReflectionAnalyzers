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
        public GetMethod()
        {
            typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(PublicPrivateStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(this.PublicInstanceMethod), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(PrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(this.PrivateInstanceMethod), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(this.PublicPrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);

            typeof(GetMethod).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new[] { typeof(object), typeof(object) }, null);
            typeof(GetMethod).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(ProtectedStatic), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
            typeof(GetMethod).GetMethod(nameof(this.ProtectedInstance), BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        public static int PublicStaticMethod() => 0;

        public static int PublicPrivateStaticMethod() => 0;

        public int PublicInstanceMethod() => 0;

        private static int PrivateStaticMethod() => 0;

        private int PrivateInstanceMethod() => 0;

        private static int PublicPrivateStaticMethod(int i) => i;
    }
}
