// ReSharper disable All
namespace ValidCode
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using NUnit.Framework;

    public class ConstructorInfoInvoke
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(ConstructorInfoInvoke).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(null));
            Assert.NotNull(typeof(ConstructorInfoInvoke).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null).Invoke(new object[] { 1 }));

            var type = typeof(ConstructorInfoInvoke);
            var instance = FormatterServices.GetUninitializedObject(type);
            Assert.Null(type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(instance, null));
            Assert.Null(type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null).Invoke(instance, new object[] { 1 }));
        }

        public ConstructorInfoInvoke()
        {
        }

        public ConstructorInfoInvoke(int value)
        {
        }
    }
}
