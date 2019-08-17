namespace ValidCode
{
    using System.Reflection;
    using NUnit.Framework;

    public class RedundantFlagsWhenTypeIsNotInSource
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase));
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));

            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));
            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));
            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));
            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase, null, new[] { typeof(string) }, null));
            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));

            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase));
            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }
    }
}
