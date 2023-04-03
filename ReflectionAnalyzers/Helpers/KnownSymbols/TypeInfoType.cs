namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class TypeInfoType : QualifiedType
{
    internal new readonly QualifiedMethod GetType;

    internal TypeInfoType()
        : base("System.Reflection.TypeInfo")
    {
        this.GetType = new QualifiedMethod(this, nameof(this.GetType));
    }
}
