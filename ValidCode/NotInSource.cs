#pragma warning disable CS0169
namespace ValidCode
{
    using System;
    using System.Reflection;

    public class CustomAggregateException : AggregateException
    {
        private readonly int value;
    }

    class NotInSource
    {
        public NotInSource()
        {
            _ = typeof(AggregateException).GetProperty("InnerExceptionCount", BindingFlags.NonPublic | BindingFlags.Instance);
            _ = typeof(AggregateException).GetProperty("InnerExceptionCount", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(CustomAggregateException).GetProperty("InnerExceptionCount", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
