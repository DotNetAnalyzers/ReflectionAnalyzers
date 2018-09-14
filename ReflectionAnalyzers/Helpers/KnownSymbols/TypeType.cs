namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class TypeType : QualifiedType
    {
        internal readonly QualifiedMethod GetMethod;

        internal TypeType()
            : base("System.Type")
        {
            this.GetMethod = new QualifiedMethod(this, nameof(this.GetMethod));
        }
    }
}
