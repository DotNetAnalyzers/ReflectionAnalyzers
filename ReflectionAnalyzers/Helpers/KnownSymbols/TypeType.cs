namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class TypeType : QualifiedType
    {
        internal readonly QualifiedMethod GetConstructor;
        internal readonly QualifiedMethod GetEvent;
        internal readonly QualifiedMethod GetField;
        internal readonly QualifiedMethod GetMember;
        internal readonly QualifiedMethod GetMethod;
        internal readonly QualifiedMethod GetNestedType;
        internal readonly QualifiedMethod GetProperty;

        internal TypeType()
            : base("System.Type")
        {
            this.GetConstructor = new QualifiedMethod(this, nameof(this.GetConstructor));
            this.GetEvent = new QualifiedMethod(this, nameof(this.GetEvent));
            this.GetField = new QualifiedMethod(this, nameof(this.GetField));
            this.GetMember = new QualifiedMethod(this, nameof(this.GetMember));
            this.GetNestedType = new QualifiedMethod(this, nameof(this.GetNestedType));
            this.GetMethod = new QualifiedMethod(this, nameof(this.GetMethod));
            this.GetProperty = new QualifiedMethod(this, nameof(this.GetProperty));
        }
    }
}
