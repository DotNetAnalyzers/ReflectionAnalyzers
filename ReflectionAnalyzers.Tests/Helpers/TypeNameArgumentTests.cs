namespace ReflectionAnalyzers.Tests.Helpers
{
    using System.Linq;
    using NUnit.Framework;

    public class TypeNameArgumentTests
    {
        [TestCase("System.Int32")]
        [TestCase("System.Nullable`1")]
        public void TryGetGeneric(string name)
        {
            var typeName = new TypeNameArgument(null, name);
            Assert.AreEqual(false, typeName.TryGetGeneric(out _, out _, out _));
        }

        [TestCase("System.Nullable`1[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]")]
        [TestCase("System.Nullable`1 [System.Int32]")]
        [TestCase("System.Collections.Generic.KeyValuePair`2 [System.Int32,System.String]")]
        public void TryGetGenericWhenFalse(string name)
        {
            var typeName = new TypeNameArgument(null, name);
            Assert.AreEqual(false, typeName.TryGetGeneric(out _, out _, out _));
        }

        [TestCase("System.Nullable`1[System.Int32]",                                                                                "System.Int32")]
        [TestCase("System.Nullable`1[[System.Int32]]",                                                                              "System.Int32")]
        [TestCase("System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32")]
        public void TryGetGenericWhenNullable(string name, string arg)
        {
            var typeName = new TypeNameArgument(null, name);
            Assert.AreEqual(true, typeName.TryGetGeneric(out var metadataName, out var arity, out var typeArgs));
            Assert.AreEqual("System.Nullable`1", metadataName);
            Assert.AreEqual(1, arity);
            var typeArgument = typeArgs.Single();
            Assert.AreEqual(arg, typeArgument.MetadataName);
            Assert.AreEqual(null, typeArgument.TypeArguments);
        }

        [TestCase("System.Collections.Generic.KeyValuePair`2[System.Int32,System.String]", "System.Int32", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[System.Int32, System.String]", "System.Int32", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[System.Int32,  System.String]", "System.Int32", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[ System.Int32,System.String]", "System.Int32", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[ System.Int32, System.String]", "System.Int32", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[System.Int32,System.String ]", "System.Int32", "System.String ")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[System.Int32 ,System.String]", "System.Int32 ", "System.String")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[ System.Int32, System.String ]", "System.Int32", "System.String ")]
        [TestCase("System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32", "System.String")]
        public void TryGetGenericWhenKeyValuePair(string name, string arg0, string arg1)
        {
            var typeName = new TypeNameArgument(null, name);
            if (arg0 != null)
            {
                Assert.AreEqual(true, typeName.TryGetGeneric(out var metadataName, out var arity, out var typeArgs));
                Assert.AreEqual("System.Collections.Generic.KeyValuePair`2", metadataName);
                Assert.AreEqual(2, arity);
                Assert.AreEqual(2, typeArgs.Length);
                Assert.AreEqual(arg0, typeArgs[0].MetadataName);
                Assert.AreEqual(null, typeArgs[0].TypeArguments);
                Assert.AreEqual(arg1, typeArgs[1].MetadataName);
                Assert.AreEqual(null, typeArgs[1].TypeArguments);
            }
            else
            {
                Assert.AreEqual(false, typeName.TryGetGeneric(out _, out _, out _));
            }
        }

        [TestCase("System.Nullable`1[System.Collections.Generic.KeyValuePair`2[System.Int32,System.String]]", "System.Int32", "System.String")]
        [TestCase("System.Nullable`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "System.Int32", "System.String")]
        public void TryGetGenericWhenNullableKeyValuePair(string name, string arg0, string arg1)
        {
            var typeName = new TypeNameArgument(null, name);
            Assert.AreEqual(true, typeName.TryGetGeneric(out var metadataName, out var arity, out var typeArgs));
            Assert.AreEqual("System.Nullable`1", metadataName);
            Assert.AreEqual(1, arity);
            var typeArgument = typeArgs.Single();
            Assert.AreEqual("System.Collections.Generic.KeyValuePair`2", typeArgument.MetadataName);
            var genericArguments = typeArgument.TypeArguments;
            Assert.AreEqual(2, genericArguments.Count);
            Assert.AreEqual(arg0, typeArgs[0].MetadataName);
            Assert.AreEqual(null, typeArgs[0].TypeArguments);
            Assert.AreEqual(arg1, typeArgs[1].MetadataName);
            Assert.AreEqual(null, typeArgs[1].TypeArguments);
        }
    }
}
