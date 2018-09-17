namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class ConstructorInfoType : QualifiedType
    {
        internal ConstructorInfoType()
            : base("System.Reflection.ConstructorInfo")
        {
        }
    }
}