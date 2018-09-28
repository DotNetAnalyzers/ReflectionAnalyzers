namespace ReflectionAnalyzers.Tests.Helpers
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    public class BindingFlagsTests
    {
        private static readonly System.Reflection.BindingFlags[] SystemReflectionFlags = Enum.GetValues(typeof(System.Reflection.BindingFlags))
                                                                                        .Cast<System.Reflection.BindingFlags>()
                                                                                        .ToArray();

        [TestCaseSource(nameof(SystemReflectionFlags))]
        public void IsMatched(System.Reflection.BindingFlags system)
        {
            Assert.AreEqual(true, Enum.TryParse<BindingFlags>(system.ToString("G"), out var local));
            Assert.AreEqual(system.ToString("D"), local.ToString("D"));
        }
    }
}
