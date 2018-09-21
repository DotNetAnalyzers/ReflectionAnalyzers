namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class AssemblyType : QualifiedType
    {
        internal new readonly QualifiedMethod GetType;

        internal AssemblyType()
            : base("System.Reflection.Assembly")
        {
            this.GetType = new QualifiedMethod(this, nameof(this.GetType));
        }
    }
}
