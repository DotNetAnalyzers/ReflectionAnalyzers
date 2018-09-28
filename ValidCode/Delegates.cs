namespace ValidCode
{
    using System;
    using System.Reflection;

    public class Delegates
    {
        public Delegates(Delegate @delegate, Action action, Action<int> actionInt, Func<int> funcInt, Func<int, int> funcIntInt)
        {
            _ = @delegate.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = action.GetType().GetMethod(nameof(action.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            _ = action.GetType().GetMethod(nameof(Action.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            
            _ = actionInt.GetType().GetMethod(nameof(actionInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            _ = actionInt.GetType().GetMethod(nameof(Action<int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            
            _ = funcInt.GetType().GetMethod(nameof(funcInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            _ = funcInt.GetType().GetMethod(nameof(Func<int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        
            _ = funcIntInt.GetType().GetMethod(nameof(funcIntInt.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
            _ = funcIntInt.GetType().GetMethod(nameof(Func<int,int>.Invoke), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(int) }, null);
        }
    }
}
