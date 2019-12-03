namespace ValidCode.Repros
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public delegate void OnModelHandler(object e);

    public interface IThing
    {
        event OnModelHandler OnModel;
    }

    public class Thing : IThing
    {
        public event OnModelHandler OnModel;
    }

    class Issue206
    {
        [Test]
        public void Test()
        {
            var thing = new Thing();
            thing.OnModel += x => { };
            Assert.NotNull(GetDelegate(thing));
            Assert.NotNull(GetEvent(thing));
        }

        private static MulticastDelegate GetDelegate(IThing sender)
        {
            return (MulticastDelegate)sender.GetType()
                                            .GetField(nameof(IThing.OnModel), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                            .GetValue(sender);
        }

        private static EventInfo GetEvent(IThing sender)
        {
            return sender.GetType()
                         .GetEvent(nameof(IThing.OnModel), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
