namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class ObjectType : QualifiedType
    {
        internal new readonly QualifiedMethod Equals;
        internal new readonly QualifiedMethod GetType;
        internal new readonly QualifiedMethod ReferenceEquals;

        internal ObjectType()
            : base("System.Object", "object")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
            this.GetType = new QualifiedMethod(this, nameof(this.GetType));
            this.ReferenceEquals = new QualifiedMethod(this, nameof(this.ReferenceEquals));
        }
    }
}
