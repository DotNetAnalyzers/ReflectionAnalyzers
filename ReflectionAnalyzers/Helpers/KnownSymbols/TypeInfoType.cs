namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class TypeInfoType : QualifiedType
    {
        internal TypeInfoType()
            : base("System.Reflection.TypeInfo")
        {
        }
    }
}