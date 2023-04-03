namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class EventInfoType : QualifiedType
{
    internal EventInfoType()
        : base("System.Reflection.EventInfo")
    {
    }
}
