namespace ValidCode
{
    using NUnit.Framework;
    using System.Reflection;

    public class Operators
    {
        [Test]
        public void Valid()
        {
            Assert.AreEqual(1, typeof(Operators).GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null }));
            Assert.AreEqual(true, typeof(Operators).GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null }));
            Assert.AreEqual(true, typeof(Operators).GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null }));
            Assert.AreEqual(2, typeof(Operators).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators) }, null).Invoke(null, new object[] { (Operators)null }));
            Assert.Null(typeof(Operators).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null).Invoke(null, new object[] { 1 }));
        }

        public static int operator +(Operators left, Operators right) => 1;

        public static bool operator ==(Operators left, Operators right) => true;

        public static bool operator !=(Operators left, Operators right) => true;

        public static explicit operator int(Operators c) => 2;

        public static explicit operator Operators(int c) => null;
    }
}
