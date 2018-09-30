#pragma warning disable CS0169
namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }

    class NotInSource
    {
        [Test]
        public void Valid()
        {
            const string Innerexceptioncount = "InnerExceptionCount";
            Assert.NotNull(typeof(AggregateException).GetProperty(Innerexceptioncount, BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.NotNull(typeof(AggregateException).GetProperty(Innerexceptioncount, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(CustomAggregateException).GetProperty(Innerexceptioncount, BindingFlags.NonPublic | BindingFlags.Instance));
        }
    }
}
