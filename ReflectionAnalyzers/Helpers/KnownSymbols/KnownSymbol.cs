namespace ReflectionAnalyzers
{
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly StringType String = new StringType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");

        internal static readonly TypeType Type = new TypeType();
        internal static readonly AttributeType Attribute = new AttributeType();
        internal static readonly QualifiedType BindingFlags = Create(typeof(BindingFlags).FullName);
        internal static readonly MemberInfoType MemberInfo = new MemberInfoType();
        internal static readonly MethodInfoType MethodInfo = new MethodInfoType();

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
