namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class PropertyInfoType : QualifiedType
    {
        internal PropertyInfoType()
            : base("System.Reflection.PropertyInfo")
        {
        }
    }
}