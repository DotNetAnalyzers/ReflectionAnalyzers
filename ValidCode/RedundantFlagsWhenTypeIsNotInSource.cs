namespace ValidCode
{
    using System.Reflection;

    public class RedundantFlagsWhenTypeIsNotInSource
    {
        public RedundantFlagsWhenTypeIsNotInSource()
        {
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            _ = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            _ = typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            _ = typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
