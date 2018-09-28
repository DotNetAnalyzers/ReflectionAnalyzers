// ReSharper disable All
namespace ValidCode
{
    using System;

    public class ActivatorCreateInstance
    {
        public ActivatorCreateInstance()
        {
            _ = Activator.CreateInstance<ImplicitDefaultConstructor>();
            _ = (ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor));
            _ = (ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor), true);
            _ = (ImplicitDefaultConstructor)Activator.CreateInstance(typeof(ImplicitDefaultConstructor), false);

            _ = Activator.CreateInstance<ExplicitDefaultConstructor>();
            _ = (ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor));
            _ = (ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor), true);
            _ = (ExplicitDefaultConstructor)Activator.CreateInstance(typeof(ExplicitDefaultConstructor), false);

            _ = (PrivateDefaultConstructor)Activator.CreateInstance(typeof(PrivateDefaultConstructor), true);

            _ = (SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), 1);
            _ = (SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), 1.2);
            _ = (SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), new object[] { 1 });
            _ = (SingleDoubleParameter)Activator.CreateInstance(typeof(SingleDoubleParameter), new object[] { 1.2 });
        }

        public T Create<T>() => Activator.CreateInstance<T>();

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
