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
            Assert.NotNull(typeof(Foo<>).MakeGenericType(typeof(string)));
            Assert.NotNull(typeof(Foo<>.Bar).MakeGenericType(typeof(int)));
            Assert.NotNull(typeof(ConstrainedToIComparableOfT<>).MakeGenericType(typeof(int)));
            Assert.NotNull(typeof(ConstrainedToClass<>).MakeGenericType(typeof(string)));
            Assert.NotNull(typeof(ConstrainedToStruct<>).MakeGenericType(typeof(int)));
        }

        public Type GetTernary<T>()
        {
            return typeof(T).IsValueType
                ? typeof(MakeGenericType).GetNestedType("ConstrainedToStruct`1", BindingFlags.Public).MakeGenericType(typeof(T))
                : typeof(MakeGenericType).GetNestedType("ConstrainedToClass`1", BindingFlags.Public).MakeGenericType(typeof(T));
        }

        public class Foo<T>
        {
            public class Bar
            {
            }
        }

        public class ConstrainedToIComparableOfT<T>
            where T : IComparable<T>
        {
        }

        public class ConstrainedToClass<T>
            where T : class
        {
        }

        public class ConstrainedToStruct<T>
            where T : struct
        {
        }
    }
}
