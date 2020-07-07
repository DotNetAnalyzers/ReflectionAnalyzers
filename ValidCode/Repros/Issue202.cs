// ReSharper disable All
namespace ValidCode
{
    using System;
    using System.Reflection;

    internal static class Mapper
    {
        internal static IMapper<TSource, TResult> Create<TSource, TResult>(Func<TSource, TResult> selector)
        {
            if (!typeof(TSource).IsValueType &&
                !typeof(TResult).IsValueType)
            {
                return (IMapper<TSource, TResult>)Activator.CreateInstance(
                typeof(CreatingCaching<,>).MakeGenericType(typeof(TSource), typeof(TResult)),
                new object[] { selector });
            }

            return new Creating<TSource, TResult>(selector);
        }

        internal static IMapper<TSource, TResult> Create<TSource, TResult>(
            Func<TSource, int, TResult> selector,
            Func<TResult, int, TResult> updater)
        {
            return new Updating<TSource, TResult>(selector, updater);
        }

        internal static IMapper<TSource, TResult> Create<TSource, TResult>(
            Func<TSource, TResult> selector,
            Action<TResult> onRemove)
            where TResult : class
        {
            var type = typeof(TSource).IsValueType
                ? typeof(CreatingRemoving<,>).MakeGenericType(typeof(TSource), typeof(TResult))
                : typeof(CreatingCachingRemoving<,>).MakeGenericType(typeof(TSource), typeof(TResult));

            var args = new object[] { selector, onRemove };
            var constructor = type.GetConstructor(
                bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                binder: null,
                types: Type.GetTypeArray(args),
                modifiers: null);
            return (IMapper<TSource, TResult>)constructor.Invoke(args);
        }

        internal static IMapper<TSource, TResult> Create<TSource, TResult>(
            Func<TSource, int, TResult> indexSelector,
            Func<TResult, int, TResult> indexUpdater,
            Action<TResult> onRemove)
            where TResult : class
        {
            return new UpdatingRemoving<TSource, TResult>(indexSelector, indexUpdater, onRemove);
        }
    }

    internal interface IMapper<in TSource, TResult>
    {
    }

    internal sealed class Creating<TSource, TResult> : Updating<TSource, TResult>
    {
        internal Creating(Func<TSource, TResult> selector)
            : base((o, _) => selector(o))
        {
        }
    }

    internal class Updating<TSource, TResult> : IMapper<TSource, TResult>
    {
        private Func<TSource, int, TResult> selector;
        private Func<TResult, int, TResult>? updater;

        public Updating(Func<TSource, int, TResult> selector, Func<TResult, int, TResult>? updater)
        {
            this.selector = selector;
            this.updater = updater;
        }

        protected Updating(Func<TSource, int, TResult> selector)
            : this(selector, null)
        {
        }
    }

    internal class UpdatingRemoving<TSource, TResult> : IMapper<TSource, TResult>
        where TResult : class
    {
        private Func<TSource, int, TResult> indexSelector;
        private Func<TResult, int, TResult> indexUpdater;
        private Action<TResult> onRemove;

        public UpdatingRemoving(Func<TSource, int, TResult> indexSelector, Func<TResult, int, TResult> indexUpdater, Action<TResult> onRemove)
        {
            this.indexSelector = indexSelector;
            this.indexUpdater = indexUpdater;
            this.onRemove = onRemove;
        }
    }

    internal class CreatingRemoving<TSource, TResult> : IMapper<TSource, TResult>
        where TSource : struct
        where TResult : class
    {
    }

    internal class CreatingCaching<TSource, TResult> : IMapper<TSource, TResult>
        where TSource : class
        where TResult : class
    {
    }

    internal class CreatingCachingRemoving<TSource, TResult> : CreatingCaching<TSource, TResult>
        where TSource : class
        where TResult : class
    {
    }
}
