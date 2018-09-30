// ReSharper disable All
namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;
    using static System.Reflection.BindingFlags;

    class Foo
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(Foo).GetMethod(nameof(this.Bar), Public | Instance | DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(Foo).GetMethod(nameof(this.Bar), Public | BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(typeof(Foo).GetMethod(nameof(this.Bar), Public | System.Reflection.BindingFlags.Instance | DeclaredOnly, null, Type.EmptyTypes, null));
        }

        public int Bar() => 0;
    }
}
