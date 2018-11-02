// ReSharper disable All
namespace ValidCode
{
    using NUnit.Framework;
    using System.Reflection;

    public class GetNestedType
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(GetType().GetNestedType(nameof(PublicStatic), BindingFlags.Public));
            Assert.NotNull(this.GetType().GetNestedType(nameof(PublicStatic), BindingFlags.Public));
            Assert.NotNull(this?.GetType().GetNestedType(nameof(PublicStatic), BindingFlags.Public));
            Assert.NotNull(typeof(GetNestedType).GetNestedType(nameof(PublicStatic), BindingFlags.Public));
            Assert.NotNull(typeof(GetNestedType).GetNestedType(nameof(Public), BindingFlags.Public));
            Assert.NotNull(typeof(GetNestedType).GetNestedType(nameof(PrivateStatic), BindingFlags.NonPublic));
            Assert.NotNull(typeof(GetNestedType).GetNestedType(nameof(Private), BindingFlags.NonPublic));
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
