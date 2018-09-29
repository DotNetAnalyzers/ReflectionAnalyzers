namespace ValidCode
{
    using System.Reflection;

    public class Operators
    {
        public Operators()
        {
            _ = typeof(Operators).GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null });
            _ = typeof(Operators).GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null });
            _ = typeof(Operators).GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators), typeof(Operators) }, null).Invoke(null, new object[] { null, null });
            _ = typeof(Operators).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null).Invoke(null, new object[] { 1 });
            _ = typeof(Operators).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Operators) }, null).Invoke(null, new object[] { (Operators)null });
        }

        public static Operators operator +(Operators left, Operators right) => null;

        public static bool operator ==(Operators left, Operators right) => false;

        public static bool operator !=(Operators left, Operators right) => false;

        public static explicit operator int(Operators c) => 0;

        public static explicit operator Operators(int c) => null;
    }
}
