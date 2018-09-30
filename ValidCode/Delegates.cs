namespace ValidCode
{
    using NUnit.Framework;
    using System;
    using System.Reflection;

    public class Delegates
    {
        [Test]
        public void Valid()
        {
            Delegate @delegate = Create();
            Action action = () => { };
            Action<int> actionInt = _ => { };
            Func<int> funcInt = () => 0;
            Func<int, int> funcIntInt = x => x;
            Assert.NotNull(@delegate.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.NotNull(action.GetType().GetMethod(nameof(action.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(action.GetType().GetMethod(nameof(Action.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));

            Assert.NotNull(actionInt.GetType().GetMethod(nameof(actionInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null));
            Assert.NotNull(actionInt.GetType().GetMethod(nameof(Action<int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null));

            Assert.NotNull(funcInt.GetType().GetMethod(nameof(funcInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));
            Assert.NotNull(funcInt.GetType().GetMethod(nameof(Func<int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));

            Assert.NotNull(funcIntInt.GetType().GetMethod(nameof(funcIntInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null));
            Assert.NotNull(funcIntInt.GetType().GetMethod(nameof(Func<int, int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null));
        }

        private static Delegate Create() => new Action(delegate { });
    }
}
