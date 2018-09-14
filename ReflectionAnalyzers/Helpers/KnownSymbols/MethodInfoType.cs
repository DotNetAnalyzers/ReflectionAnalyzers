namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MethodInfoType : QualifiedType
    {
        internal MethodInfoType()
            : base("System.Reflection.MethodInfo")
        {
        }
    }
}