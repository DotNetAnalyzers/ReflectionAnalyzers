namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class AttributeType : QualifiedType
{
    internal readonly QualifiedMethod GetCustomAttribute;
    internal readonly QualifiedMethod IsDefined;

    internal AttributeType()
        : base("System.Attribute")
    {
        this.GetCustomAttribute = new QualifiedMethod(this, nameof(this.GetCustomAttribute));
        this.IsDefined = new QualifiedMethod(this, nameof(this.IsDefined));
    }
}
