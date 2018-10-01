// ReSharper disable All
namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;

    public class Accessors
    {
        public event EventHandler Baz;

        public int Bar { get; set; }

        [Test]
        public void Valid()
        {
            var instance = new Accessors { Bar = 1 };
#pragma warning disable REFL014
            Assert.NotNull(typeof(Accessors).GetMethod("get_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.AreEqual(1, typeof(Accessors).GetMethod("get_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(instance, null));

            Assert.NotNull(typeof(Accessors).GetMethod("set_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.Null(typeof(Accessors).GetMethod("set_Bar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(instance, new object[] { 1 }));

            Assert.NotNull(typeof(Accessors).GetMethod("add_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.Null(typeof(Accessors).GetMethod("add_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(instance, new object[] { new EventHandler((_, __) => { }) }));

            Assert.NotNull(typeof(Accessors).GetMethod("remove_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.Null(typeof(Accessors).GetMethod("remove_Baz", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(instance, new object[] { new EventHandler((_, __) => { }) }));
#pragma warning restore REFL014
        }
    }
}
