namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly StringType String = new StringType();
        internal static readonly ArrayType Array = new ArrayType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType Delegate = Create("System.Delegate", "bool");

        internal static readonly TypeType Type = new TypeType();
        internal static readonly ActivatorType Activator = new ActivatorType();
        internal static readonly AttributeType Attribute = new AttributeType();
        internal static readonly AssemblyType Assembly = new AssemblyType();
        internal static readonly QualifiedType BindingFlags = Create("System.Reflection.BindingFlags");
        internal static readonly QualifiedType Binder = Create("System.Reflection.Binder");
        internal static readonly QualifiedType ParameterModifier = Create("System.Reflection.ParameterModifier");
        internal static readonly QualifiedType CallingConventions = Create("System.Reflection.CallingConventions");
        internal static readonly QualifiedType DependencyAttribute = Create("System.Runtime.CompilerServices.DependencyAttribute", "Dependency");
        internal static readonly ConstructorInfoType ConstructorInfo = new ConstructorInfoType();
        internal static readonly EventInfoType EventInfo = new EventInfoType();
        internal static readonly FieldInfoType FieldInfo = new FieldInfoType();
        internal static readonly LocalVariableInfoType LocalVariableInfo = new LocalVariableInfoType();
        internal static readonly ManifestResourceInfoType ManifestResourceInfo = new ManifestResourceInfoType();
        internal static readonly MemberInfoType MemberInfo = new MemberInfoType();
        internal static readonly MethodInfoType MethodInfo = new MethodInfoType();
        internal static readonly ParameterInfoType ParameterInfo = new ParameterInfoType();
        internal static readonly PropertyInfoType PropertyInfo = new PropertyInfoType();
        internal static readonly RuntimeConstructorInfoType RuntimeConstructorInfo = new RuntimeConstructorInfoType();
        internal static readonly RuntimeEventInfoType RuntimeEventInfo = new RuntimeEventInfoType();
        internal static readonly RuntimeFieldInfoType RuntimeFieldInfo = new RuntimeFieldInfoType();
        internal static readonly RuntimeMethodInfoType RuntimeMethodInfo = new RuntimeMethodInfoType();
        internal static readonly RuntimeParameterInfoType RuntimeParameterInfo = new RuntimeParameterInfoType();
        internal static readonly RuntimePropertyInfoType RuntimePropertyInfo = new RuntimePropertyInfoType();
        internal static readonly TypeInfoType TypeInfo = new TypeInfoType();

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
