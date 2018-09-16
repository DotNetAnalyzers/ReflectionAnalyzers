namespace ValidCode
{
    using System.Reflection;

    public class GetMethodOverloads
    {
        public GetMethodOverloads()
        {
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicStatic), new[] { typeof(int) });
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicStatic), new[] { typeof(double) });
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicStatic), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(double) }, null);

            typeof(GetMethodOverloads).GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Static);
            typeof(GetMethodOverloads).GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Instance);
            typeof(GetMethodOverloads).GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            typeof(GetMethodOverloads).GetMethod(nameof(PublicStaticInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicInstance), new[] { typeof(int) });
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicInstance), new[] { typeof(double) });
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) },    null);
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicInstance), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null);

            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.Public | BindingFlags.Instance);
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.NonPublic | BindingFlags.Instance);
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            typeof(GetMethodOverloads).GetMethod(nameof(this.PublicPrivateInstance), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static int PublicStatic(int value) => value;

        public static double PublicStatic(double value) => value;

        public static int PublicStaticInstance(int value) => value;

        public double PublicStaticInstance(double value) => value;

        public int PublicInstance(int value) => value;

        public double PublicInstance(double value) => value;

        public int PublicPrivateInstance(int value) => value;

        private double PublicPrivateInstance(double value) => value;
    }
}
