// ReSharper disable All
namespace ValidCode
{
    using System.Reflection;

    public class AssemblyLoad
    {
        public AssemblyLoad()
        {
            _ = (string?)Assembly.Load("SomeAssembly").GetType("SomeType")?.GetMethod("SomeMethod").Invoke(null, new object[] { 1 });
            _ = (string)Assembly.Load("SomeAssembly").GetType("SomeType", throwOnError: true).GetMethod("SomeMethod").Invoke(null, new object[] { 1 });
            _ = (string)Assembly.Load("SomeAssembly, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e000\"").GetType("SomeType", throwOnError: true).GetMethod("SomeMethod").Invoke(null, new object[] { 1 });
        }
    }
}
