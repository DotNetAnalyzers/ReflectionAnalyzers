namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class AttributeType : QualifiedType
    {
        internal readonly QualifiedMethod GetCustomAttribute;

        internal AttributeType()
            : base("System.Attribute")
        {
            this.GetCustomAttribute = new QualifiedMethod(this, nameof(this.GetCustomAttribute));
        }
    }
}
