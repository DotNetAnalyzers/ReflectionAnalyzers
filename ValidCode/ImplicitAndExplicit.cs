namespace ValidCode
{
    using System;
    using System.Reflection;

    public interface IExplicitImplicit
    {
        event EventHandler Bar;
    }

    public class ExplicitImplicit : IExplicitImplicit
    {
        public ExplicitImplicit()
        {
            _ = typeof(ExplicitImplicit).GetEvent(nameof(this.Bar), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _ = typeof(IExplicitImplicit).GetEvent(nameof(IExplicitImplicit.Bar), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        internal event EventHandler Bar;

        event EventHandler IExplicitImplicit.Bar
        {
            add => this.Bar += value;
            remove => this.Bar -= value;
        }
    }
}
