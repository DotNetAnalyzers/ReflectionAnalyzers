namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class PropertyInfoType : QualifiedType
{
    internal readonly QualifiedProperty GetMethod;
    internal readonly QualifiedMethod GetGetMethod;
    internal readonly QualifiedMethod GetSetMethod;
    internal readonly QualifiedProperty SetMethod;

    internal PropertyInfoType()
        : base("System.Reflection.PropertyInfo")
    {
        this.GetMethod = new QualifiedProperty(this, nameof(this.GetMethod));
        this.GetGetMethod = new QualifiedMethod(this, nameof(this.GetGetMethod));
        this.GetSetMethod = new QualifiedMethod(this, nameof(this.GetSetMethod));
        this.SetMethod = new QualifiedProperty(this, nameof(this.SetMethod));
    }
}
