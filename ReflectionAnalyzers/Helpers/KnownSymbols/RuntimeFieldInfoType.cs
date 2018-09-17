namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class RuntimeFieldInfoType : QualifiedType
    {
        internal RuntimeFieldInfoType()
            : base("System.Reflection.RuntimeFieldInfo")
        {
        }
    }
}