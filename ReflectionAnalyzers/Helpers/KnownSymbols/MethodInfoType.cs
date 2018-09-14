namespace ReflectionAnalyzers
{
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MethodInfoType : QualifiedType
    {
        internal MethodInfoType()
            : base(typeof(MethodInfo).FullName)
        {
        }
    }
}
