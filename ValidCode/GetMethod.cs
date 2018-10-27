// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable All
namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;

    public class Base
    {
        protected static void ProtectedStatic() { }

        protected void ProtectedInstance() { }
    }

    public class GetMethod : Base
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, Type.DefaultBinder, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PublicPrivateStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(this.PublicInstanceMethod), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(PrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(this.PrivateInstanceMethod), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(this.PublicPrivateStaticMethod), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null));

            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(ReferenceEquals), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new[] { typeof(object), typeof(object) }, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(this.ToString), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(ProtectedStatic), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(GetMethod).GetMethod(nameof(this.ProtectedInstance), BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));

            int? nullableInt = 1;
#pragma warning disable REFL039 // Prefer typeof(...) over instance.GetType when the type is sealed.
            Assert.NotNull(nullableInt.GetType().GetMethod(nameof(int.Parse), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));
            Assert.NotNull(nullableInt.GetType().GetField(nameof(int.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
#pragma warning restore REFL039 // Prefer typeof(...) over instance.GetType when the type is sealed.
        }

        public static int PublicStaticMethod() => 0;

        public static int PublicPrivateStaticMethod() => 0;

        public int PublicInstanceMethod() => 0;

        private static int PrivateStaticMethod() => 0;

        private int PrivateInstanceMethod() => 0;

        private static int PublicPrivateStaticMethod(int i) => i;
    }
}
