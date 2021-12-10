namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new();
        internal static readonly StringType String = new();
        internal static readonly ArrayType Array = new();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly DelegateType Delegate = new();
        internal static readonly QualifiedType NullableOfT = Create("System.Nullable`1");

        internal static readonly TypeType Type = new();
        internal static readonly ActivatorType Activator = new();
        internal static readonly AttributeType Attribute = new();
        internal static readonly AssemblyType Assembly = new();
        internal static readonly QualifiedType BindingFlags = Create("System.Reflection.BindingFlags");
        internal static readonly QualifiedType Binder = Create("System.Reflection.Binder");
        internal static readonly MissingType Missing = new();
        internal static readonly CustomAttributeExtensionsType CustomAttributeExtensions = new();
        internal static readonly QualifiedType ParameterModifier = Create("System.Reflection.ParameterModifier");
        internal static readonly QualifiedType CallingConventions = Create("System.Reflection.CallingConventions");
        internal static readonly QualifiedType DependencyAttribute = Create("System.Runtime.CompilerServices.DependencyAttribute", "Dependency");
        internal static readonly QualifiedType DefaultMemberAttribute = Create("System.Reflection.DefaultMemberAttribute", "DefaultMember");
        internal static readonly QualifiedType IndexerNameAttribute = Create("System.Runtime.CompilerServices.IndexerNameAttribute", "IndexerName");
        internal static readonly ConstructorInfoType ConstructorInfo = new();
        internal static readonly EventInfoType EventInfo = new();
        internal static readonly FieldInfoType FieldInfo = new();
        internal static readonly LocalVariableInfoType LocalVariableInfo = new();
        internal static readonly ManifestResourceInfoType ManifestResourceInfo = new();
        internal static readonly MemberInfoType MemberInfo = new();
        internal static readonly MethodInfoType MethodInfo = new();
        internal static readonly ParameterInfoType ParameterInfo = new();
        internal static readonly PropertyInfoType PropertyInfo = new();
        internal static readonly RuntimeConstructorInfoType RuntimeConstructorInfo = new();
        internal static readonly RuntimeEventInfoType RuntimeEventInfo = new();
        internal static readonly RuntimeFieldInfoType RuntimeFieldInfo = new();
        internal static readonly RuntimeMethodInfoType RuntimeMethodInfo = new();
        internal static readonly RuntimeParameterInfoType RuntimeParameterInfo = new();
        internal static readonly RuntimePropertyInfoType RuntimePropertyInfo = new();
        internal static readonly TypeInfoType TypeInfo = new();
        internal static readonly AssemblyBuilderType AssemblyBuilder = new();
        internal static readonly NUnitAssertType NUnitAssert = new();

        private static QualifiedType Create(string qualifiedName, string? alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
