namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    public class BindingFlagsExtTests
    {
        private static readonly BindingFlags[] Flags = Enum.GetValues(typeof(BindingFlags))
                                                           .Cast<BindingFlags>()
                                                           .ToArray();

        [TestCaseSource(nameof(Flags))]
        public void ToDisplayString(object flags)
        {
            Assert.AreEqual("BindingFlags." + flags, ((BindingFlags)flags).ToDisplayString(null));
        }
    }
}
