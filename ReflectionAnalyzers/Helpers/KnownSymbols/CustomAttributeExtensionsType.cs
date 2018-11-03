namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class CustomAttributeExtensionsType : QualifiedType
    {
        internal readonly QualifiedMethod GetCustomAttribute;

        internal CustomAttributeExtensionsType()
            : base("System.Reflection.CustomAttributeExtensions")
        {
            this.GetCustomAttribute = new QualifiedMethod(this, nameof(this.GetCustomAttribute));
        }
    }
}