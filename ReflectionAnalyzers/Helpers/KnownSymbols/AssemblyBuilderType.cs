namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class AssemblyBuilderType : QualifiedType
    {
        internal new readonly QualifiedMethod GetType;

        internal AssemblyBuilderType()
            : base("System.Reflection.Emit.AssemblyBuilder")
        {
            this.GetType = new QualifiedMethod(this, nameof(this.GetType));
        }
    }
}
