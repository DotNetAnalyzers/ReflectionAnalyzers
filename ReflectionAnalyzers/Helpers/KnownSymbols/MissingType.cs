namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MissingType : QualifiedType
    {
        internal readonly QualifiedField Value;

        public MissingType()
            : base("System.Reflection.Missing")
        {
            this.Value = new QualifiedField(this, nameof(this.Value));
        }
    }
}