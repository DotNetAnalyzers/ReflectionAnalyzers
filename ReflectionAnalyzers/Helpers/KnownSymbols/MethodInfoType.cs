namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class MethodInfoType : QualifiedType
{
    internal readonly QualifiedMethod MakeGenericMethod;

    internal MethodInfoType()
        : base("System.Reflection.MethodInfo")
    {
        this.MakeGenericMethod = new QualifiedMethod(this, nameof(this.MakeGenericMethod));
    }
}
