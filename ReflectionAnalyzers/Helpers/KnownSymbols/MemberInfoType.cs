namespace ReflectionAnalyzers
{
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MemberInfoType : QualifiedType
    {
        internal MemberInfoType()
            : base(typeof(MemberInfo).FullName)
        {
        }
    }
}
