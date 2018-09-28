namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MethodInfoType : QualifiedType
    {
        internal readonly QualifiedMethod MakeGenericMethod;
        internal readonly QualifiedMethod MakeGenericType;

        internal MethodInfoType()
            : base("System.Reflection.MethodInfo")
        {
            this.MakeGenericMethod = new QualifiedMethod(this, nameof(this.MakeGenericMethod));
            this.MakeGenericType = new QualifiedMethod(this, nameof(this.MakeGenericType));
        }
    }
}
