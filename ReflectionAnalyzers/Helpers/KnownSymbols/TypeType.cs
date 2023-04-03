namespace ReflectionAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class TypeType : QualifiedType
{
    internal readonly QualifiedProperty DefaultBinder;
    internal readonly QualifiedField EmptyTypes;
    internal readonly QualifiedMethod GetConstructor;
    internal readonly QualifiedMethod GetEvent;
    internal readonly QualifiedMethod GetField;
    internal readonly QualifiedMethod GetInterface;
    internal readonly QualifiedMethod GetGenericTypeDefinition;
    internal readonly QualifiedMethod GetMember;
    internal readonly QualifiedMethod GetMethod;
    internal readonly QualifiedMethod GetNestedType;
    internal readonly QualifiedMethod GetProperty;
    internal new readonly QualifiedMethod GetType;
    internal readonly QualifiedMethod IsAssignableFrom;
    internal readonly QualifiedMethod MakeGenericType;

    internal TypeType()
        : base("System.Type")
    {
        this.DefaultBinder = new QualifiedProperty(this, nameof(this.DefaultBinder));
        this.EmptyTypes = new QualifiedField(this, nameof(this.EmptyTypes));
        this.GetConstructor = new QualifiedMethod(this, nameof(this.GetConstructor));
        this.GetEvent = new QualifiedMethod(this, nameof(this.GetEvent));
        this.GetField = new QualifiedMethod(this, nameof(this.GetField));
        this.GetInterface = new QualifiedMethod(this, nameof(this.GetInterface));
        this.GetGenericTypeDefinition = new QualifiedMethod(this, nameof(this.GetGenericTypeDefinition));
        this.GetMember = new QualifiedMethod(this, nameof(this.GetMember));
        this.GetNestedType = new QualifiedMethod(this, nameof(this.GetNestedType));
        this.GetMethod = new QualifiedMethod(this, nameof(this.GetMethod));
        this.GetProperty = new QualifiedMethod(this, nameof(this.GetProperty));
        this.GetType = new QualifiedMethod(this, nameof(this.GetType));
        this.IsAssignableFrom = new QualifiedMethod(this, nameof(this.IsAssignableFrom));
        this.MakeGenericType = new QualifiedMethod(this, nameof(this.MakeGenericType));
    }
}
