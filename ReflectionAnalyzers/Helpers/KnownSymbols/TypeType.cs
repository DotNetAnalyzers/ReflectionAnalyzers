namespace ReflectionAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;

    internal class TypeType : QualifiedType
    {
        internal readonly QualifiedMethod GetMethod;

        internal TypeType()
            : base(typeof(Type).FullName)
        {
            this.GetMethod = new QualifiedMethod(this, nameof(this.GetMethod));
        }
    }
}
