// ReSharper disable All
namespace ValidCode
{
    using System;
    using NUnit.Framework;

    public class ActivatorCreateInstance
    {
        [Test]
        public void Valid()
        {
            Assert.NotNull(Activator.CreateInstance<ImplicitDefaultConstructor>());
            Assert.NotNull((ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor)));
            Assert.NotNull((ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor), true));
            Assert.NotNull((ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor), false));

            Assert.NotNull(Activator.CreateInstance<ExplicitDefaultConstructor>());
            Assert.NotNull((ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor)));
            Assert.NotNull((ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor), true));
            Assert.NotNull((ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor), false));

            Assert.NotNull((PrivateDefaultConstructor)Activator.CreateInstance(typeof(PrivateDefaultConstructor), true));

            Assert.NotNull((SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), 1));
            Assert.NotNull((SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), 1.2));
            Assert.NotNull((SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), new object[] { 1 }));
            Assert.NotNull((SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), new object[] { 1.2 }));
        }

        public T Create<T>() => Activator.CreateInstance<T>();

        public static object Foo<T>(object _) => Activator.CreateInstance(typeof(T), "foo");

        public static object Foo<T>() => Activator.CreateInstance(typeof(T));

        public class ImplicitDefaultConstructor
        {
        }

        public class ExplicitDefaultConstructor
        {
            public ExplicitDefaultConstructor()
            {
            }
        }

        public class PrivateDefaultConstructor
        {
            private PrivateDefaultConstructor()
            {
            }
        }

        public class SingleDoubleParameter
        {
            public SingleDoubleParameter(double value)
            {
            }
        }
    }
}
