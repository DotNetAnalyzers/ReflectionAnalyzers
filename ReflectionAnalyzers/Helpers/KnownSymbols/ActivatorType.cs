namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class ActivatorType : QualifiedType
    {
        internal readonly QualifiedMethod CreateInstance;

        internal ActivatorType()
            : base("System.Activator")
        {
            this.CreateInstance = new QualifiedMethod(this, nameof(this.CreateInstance));
        }
    }
}
