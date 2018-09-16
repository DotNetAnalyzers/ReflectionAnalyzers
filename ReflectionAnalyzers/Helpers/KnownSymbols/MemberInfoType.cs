namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MemberInfoType : QualifiedType
    {
        internal MemberInfoType()
            : base("System.Reflection.MemberInfo")
        {
        }
    }
}
