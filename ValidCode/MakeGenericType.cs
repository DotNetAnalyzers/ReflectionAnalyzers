namespace ValidCode
{
    using System;
    using NUnit.Framework;

    public class MakeGenericType
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(typeof(Foo<>).MakeGenericType(typeof(int)));
            Assert.NotNull(typeof(Foo<>.Bar).MakeGenericType(typeof(int)));
            Assert.NotNull(typeof(Constrained<>).MakeGenericType(typeof(int)));
        }

        public class Foo<T>
        {
            public class Bar
            {
            }
        }

        public class Constrained<T>
            where T : IComparable<T>
        {
        }
    }
}
