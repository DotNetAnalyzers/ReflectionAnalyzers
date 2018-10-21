namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Linq;
    using NUnit.Framework;

    public class GenericTypeArgumentTests
    {
        [TestCase("[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]")]
        public void TryParseInvalid(string text)
        {
            Assert.AreEqual(false, GenericTypeArgument.TryParseBracketedList(text, 0, 1, out _));
        }

        [TestCase("[System.Int32]",                                                                                 "System.Int32")]
        [TestCase("[ System.Int32]",                                                                                "System.Int32")]
        [TestCase("[System.Int32 ]",                                                                                "System.Int32 ")]
        [TestCase("[[System.Int32]]",                                                                               "System.Int32")]
        [TestCase("[ [System.Int32] ]",                                                                             "System.Int32")]
        [TestCase("[[System.Int32 ]]",                                                                              "System.Int32 ")]
        [TestCase("[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",  "System.Int32")]
        [TestCase("[[ System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32")]
        [TestCase("[[System.Int32 , mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32 ")]
        public void TryParseSingle(string name, string arg)
        {
            Assert.AreEqual(true, GenericTypeArgument.TryParseBracketedList(name, 0, 1, out var args));
            var typeArgument = args.Single();
            Assert.AreEqual(arg,  typeArgument.MetadataName);
            Assert.AreEqual(null, typeArgument.TypeArguments);
        }

        [TestCase("[System.Int32,System.String]",                                                                                                                                                               "System.Int32", "System.String")]
        [TestCase("[System.Int32, System.String]",                                                                                                                                                              "System.Int32", "System.String")]
        [TestCase("[System.Int32,  System.String]",                                                                                                                                                             "System.Int32", "System.String")]
        [TestCase("[ System.Int32,System.String]",                                                                                                                                                              "System.Int32", "System.String")]
        [TestCase("[ System.Int32, System.String]",                                                                                                                                                             "System.Int32", "System.String")]
        [TestCase("[ [System.Int32] , [System.String] ]",                                                                                                                                                       "System.Int32", "System.String")]
        [TestCase("[System.Int32,System.String ]",                                                                                                                                                              "System.Int32", "System.String ")]
        [TestCase("[System.Int32 ,System.String]",                                                                                                                                                              "System.Int32 ", "System.String")]
        [TestCase("[ System.Int32, System.String ]",                                                                                                                                                            "System.Int32", "System.String ")]
        [TestCase("[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32", "System.String")]
        public void TryGetGenericWhenKeyValuePair(string name, string arg0, string arg1)
        {
            Assert.AreEqual(true, GenericTypeArgument.TryParseBracketedList(name, 0, 2, out var args));
            var typeArgument = args[0];
            Assert.AreEqual(arg0, typeArgument.MetadataName);
            Assert.AreEqual(null, typeArgument.TypeArguments);
            typeArgument = args[1];
            Assert.AreEqual(arg1, typeArgument.MetadataName);
            Assert.AreEqual(null, typeArgument.TypeArguments);
        }
    }
}
