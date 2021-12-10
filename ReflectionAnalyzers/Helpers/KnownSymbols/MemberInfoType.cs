namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MemberInfoType : QualifiedType
    {
        internal readonly QualifiedMethod Invoke;

        internal MemberInfoType()
            : base("System.Reflection.MemberInfo")
        {
            this.Invoke = new QualifiedMethod(this, nameof(this.Invoke));
        }
    }
}
