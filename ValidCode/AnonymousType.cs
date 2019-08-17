// ReSharper disable All
namespace ValidCode
{
    using System.Reflection;
    using NUnit.Framework;

    public class AnonymousType
    {
        [Test]
        public void Valid()
        {
            var anon = new { Foo = 1 };
            Assert.NotNull(anon.GetType().GetProperty(nameof(anon.Foo), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }
    }
}
