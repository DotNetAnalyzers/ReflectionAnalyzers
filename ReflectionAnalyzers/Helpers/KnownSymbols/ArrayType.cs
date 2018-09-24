namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class ArrayType : QualifiedType
    {
        internal readonly QualifiedMethod Empty;

        public ArrayType()
            : base("System.Array")
        {
            this.Empty = new QualifiedMethod(this, nameof(this.Empty));
        }
    }
}
