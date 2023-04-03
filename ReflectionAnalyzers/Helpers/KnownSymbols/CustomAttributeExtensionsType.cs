namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class CustomAttributeExtensionsType : QualifiedType
{
    internal readonly QualifiedMethod GetCustomAttribute;
    internal readonly QualifiedMethod IsDefined;

    internal CustomAttributeExtensionsType()
        : base("System.Reflection.CustomAttributeExtensions")
    {
        this.GetCustomAttribute = new QualifiedMethod(this, nameof(this.GetCustomAttribute));
        this.IsDefined = new QualifiedMethod(this, nameof(this.IsDefined));
    }
}
