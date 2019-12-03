// ReSharper disable All
namespace ValidCode.Repros
{
    using System.Reflection;

    class Issue217<T1>
    {
        private void Foo<T2>()
        {
        }

        public void M()
        {
            //                           ↓ REFL003 The type C<> does not have a member named Foo.
            //                           ↓ REFL017 Don't use name of wrong member. Expected: "Foo"
            typeof(Issue217<>).GetMethod(nameof(Foo), BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
