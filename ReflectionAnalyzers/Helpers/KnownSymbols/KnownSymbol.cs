namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly StringType String = new StringType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");

        internal static readonly MemberInfoType MemberInfo = new MemberInfoType();
        internal static readonly MethodInfoType MethodInfo = new MethodInfoType();

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
