namespace ReflectionAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;

    internal class TypeType : QualifiedType
    {
        internal readonly QualifiedMethod GetMethod;
        internal readonly QualifiedMethod GetProperty;

        internal TypeType()
            : base(typeof(Type).FullName)
        {
            this.GetMethod = new QualifiedMethod(this, nameof(this.GetMethod));
            this.GetProperty = new QualifiedMethod(this, nameof(this.GetProperty));
        }
    }
}
