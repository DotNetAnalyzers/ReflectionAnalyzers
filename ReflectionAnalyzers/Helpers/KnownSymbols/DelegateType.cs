namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class DelegateType : QualifiedType
    {
        internal readonly QualifiedMethod CreateDelegate;

        internal DelegateType()
            : base("System.Delegate", "delegate")
        {
            this.CreateDelegate = new QualifiedMethod(this, nameof(this.CreateDelegate));
        }
    }
}
