namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class RuntimeConstructorInfoType : QualifiedType
{
    internal RuntimeConstructorInfoType()
        : base("System.Reflection.RuntimeConstructorInfo")
    {
    }
}
