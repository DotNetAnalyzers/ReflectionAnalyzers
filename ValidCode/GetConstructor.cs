// ReSharper disable All
namespace ValidCode
{
    using System;
    using System.Reflection;

    public class GetConstructor
    {
        public GetConstructor()
        {
            _ = typeof(Default).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            _ = typeof(Single).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            _ = typeof(Two).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            _ = typeof(Two).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(double) }, null);
        }
        public class Default
        {
        }

        public class Single
        {
            public Single()
            {
            }
        }

        public class Two
        {
            public Two(int value)
            {
            }

            public Two(double value)
            {
            }
        }
    }
}
