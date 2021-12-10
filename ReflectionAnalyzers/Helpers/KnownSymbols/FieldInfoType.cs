namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class FieldInfoType : QualifiedType
    {
        internal FieldInfoType()
            : base("System.Reflection.FieldInfo")
        {
        }
    }
}
