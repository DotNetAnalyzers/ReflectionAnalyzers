namespace ValidCode
{
    using System.Reflection;
    using NUnit.Framework;

    public class WhenTypeIsNotInSource
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(string).GetField(nameof(string.Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

            Assert.NotNull(typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(string) }, null));

            Assert.NotNull(typeof(string).GetProperty(nameof(string.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }
    }
}
