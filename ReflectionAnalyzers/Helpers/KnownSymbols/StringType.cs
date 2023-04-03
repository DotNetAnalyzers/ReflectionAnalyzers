namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class StringType : QualifiedType
{
    internal readonly QualifiedField Empty;
    internal new readonly QualifiedMethod Equals;

    internal StringType()
        : base("System.String", "string")
    {
        this.Empty = new QualifiedField(this, nameof(this.Empty));
        this.Equals = new QualifiedMethod(this, nameof(this.Equals));
    }
}
