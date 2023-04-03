namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class LocalVariableInfoType : QualifiedType
{
    internal LocalVariableInfoType()
        : base("System.Reflection.LocalVariableInfo")
    {
    }
}
