namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class RuntimeMethodInfoType : QualifiedType
{
    internal RuntimeMethodInfoType()
        : base("System.Reflection.RuntimeMethodInfo")
    {
    }
}
