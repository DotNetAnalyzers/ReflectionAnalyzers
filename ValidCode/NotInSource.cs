namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;

    public class CustomAggregateException : AggregateException
    {
        private int InnerExceptionCount { get; }
    }

    class NotInSource
    {
        [Test]
        public void Valid()
        {
            const string InnerExceptionCount = "InnerExceptionCount";
            Assert.NotNull(typeof(AggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.NotNull(typeof(AggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(typeof(CustomAggregateException).GetProperty(InnerExceptionCount, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            Assert.NotNull(typeof(Array).GetMethod(nameof(Array.CreateInstance), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(Type), typeof(int) }, null));
        }
    }
}
