// ReSharper disable All
namespace ValidCode.Inheritance
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public class SubClass : BaseClass
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(SubClass).GetField(nameof(BaseClass.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy));
            Assert.NotNull(typeof(SubClass).GetEvent(nameof(BaseClass.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy));
            Assert.NotNull(typeof(SubClass).GetProperty(nameof(BaseClass.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy));
            Assert.NotNull(typeof(SubClass).GetMethod(nameof(BaseClass.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null));

            Assert.NotNull(typeof(BaseClass).GetField(nameof(BaseClass.PublicStaticField), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetField("PrivateStaticField", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetEvent(nameof(BaseClass.PublicStaticEvent), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetEvent("PrivateStaticEvent", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetProperty(nameof(BaseClass.PublicStaticProperty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetProperty("PrivateStaticProperty", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(BaseClass).GetMethod(nameof(BaseClass.PublicStaticMethod), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(BaseClass).GetMethod("PrivateStaticMethod", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
        }
    }
}
