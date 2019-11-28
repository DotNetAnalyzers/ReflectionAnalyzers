namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class Descriptors
    {
        internal static readonly DiagnosticDescriptor REFL001CastReturnValue = Descriptors.Create(
            id: "REFL001",
            title: "Cast return value to the correct type.",
            messageFormat: "Cast return value to the correct type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to the correct type.");

        internal static readonly DiagnosticDescriptor REFL002DiscardReturnValue = Descriptors.Create(
            id: "REFL002",
            title: "Discard the return value.",
            messageFormat: "Discard the return value.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method returns void. Discard the return value.");

        internal static readonly DiagnosticDescriptor REFL003MemberDoesNotExist = Descriptors.Create(
            id: "REFL003",
            title: "The member does not exist.",
            messageFormat: "The type {0} does not have a member named {1}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method does not exist.");

        internal static readonly DiagnosticDescriptor REFL004AmbiguousMatch = Descriptors.Create(
            id: "REFL004",
            title: "More than one member is matching the criteria.",
            messageFormat: "More than one member is matching the criteria.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "More than one member is matching the criteria.");

        internal static readonly DiagnosticDescriptor REFL005WrongBindingFlags = Descriptors.Create(
            id: "REFL005",
            title: "There is no member matching the filter.",
            messageFormat: "There is no member matching the filter.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "There is no member matching the filter.");

        internal static readonly DiagnosticDescriptor REFL006RedundantBindingFlags = Descriptors.Create(
            id: "REFL006",
            title: "The binding flags can be more precise.",
            messageFormat: "The binding flags can be more precise.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The binding flags can be more precise.");

        internal static readonly DiagnosticDescriptor REFL007BindingFlagsOrder = Descriptors.Create(
            id: "REFL007",
            title: "The binding flags are not in the expected order.",
            messageFormat: "The binding flags are not in the expected order.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "The binding flags are not in the expected order.");

        internal static readonly DiagnosticDescriptor REFL008MissingBindingFlags = Descriptors.Create(
            id: "REFL008",
            title: "Specify binding flags for better performance and less fragile code.",
            messageFormat: "Specify binding flags for better performance and less fragile code.{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Specify binding flags for better performance and less fragile code.");

        internal static readonly DiagnosticDescriptor REFL009MemberCannotBeFound = Descriptors.Create(
            id: "REFL009",
            title: "The referenced member is not known to exist.",
            messageFormat: "The referenced member {0} is not known to exist in {1}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The referenced member is not known to exist.");

        internal static readonly DiagnosticDescriptor REFL010PreferGenericGetCustomAttribute = Descriptors.Create(
            id: "REFL010",
            title: "Prefer the generic extension method GetCustomAttribute<T>.",
            messageFormat: "Prefer the generic extension method GetCustomAttribute<{0}>.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer the generic extension method GetCustomAttribute<T>.");

        internal static readonly DiagnosticDescriptor REFL011DuplicateBindingFlags = Descriptors.Create(
            id: "REFL011",
            title: "Duplicate BindingFlag.",
            messageFormat: "Duplicate flag.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Duplicate BindingFlag.");

        internal static readonly DiagnosticDescriptor REFL012PreferIsDefined = Descriptors.Create(
            id: "REFL012",
            title: "Prefer Attribute.IsDefined().",
            messageFormat: "Prefer Attribute.IsDefined().",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Attribute.IsDefined().");

        internal static readonly DiagnosticDescriptor REFL013MemberIsOfWrongType = Descriptors.Create(
            id: "REFL013",
            title: "The member is of the wrong type.",
            messageFormat: "The type {0} has a {1} named {2}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is of the wrong type.");

        internal static readonly DiagnosticDescriptor REFL014PreferGetMemberThenAccessor = Descriptors.Create(
            id: "REFL014",
            title: "Prefer GetMember().AccessorMethod.",
            messageFormat: "Prefer {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer GetMember().AccessorMethod.");

        internal static readonly DiagnosticDescriptor REFL015UseContainingType = Descriptors.Create(
            id: "REFL015",
            title: "Use the containing type.",
            messageFormat: "Use the containing type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the containing type.");

        internal static readonly DiagnosticDescriptor REFL016UseNameof = Descriptors.Create(
            id: "REFL016",
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof.");

        internal static readonly DiagnosticDescriptor REFL017NameofWrongMember = Descriptors.Create(
            id: "REFL017",
            title: "Don't use name of wrong member.",
            messageFormat: "Don't use name of wrong member. Expected: {0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use name of wrong member.");

        internal static readonly DiagnosticDescriptor REFL018ExplicitImplementation = Descriptors.Create(
            id: "REFL018",
            title: "The member is explicitly implemented.",
            messageFormat: "{0} is explicitly implemented.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The member is explicitly implemented.");

        internal static readonly DiagnosticDescriptor REFL019NoMemberMatchesTypes = Descriptors.Create(
            id: "REFL019",
            title: "No member matches the types.",
            messageFormat: "No member matches the types.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "No member matches the types.");

        internal static readonly DiagnosticDescriptor REFL020AmbiguousMatchInterface = Descriptors.Create(
            id: "REFL020",
            title: "More than one interface is matching the name.",
            messageFormat: "More than one interface is matching the name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "More than one interface is matching the name.");

        internal static readonly DiagnosticDescriptor REFL021TypeDoesNotExist = Descriptors.Create(
            id: "REFL021",
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.");

        internal static readonly DiagnosticDescriptor REFL022UseFullyQualifiedName = Descriptors.Create(
            id: "REFL022",
            title: "Use fully qualified name.",
            messageFormat: "Use fully qualified name.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use fully qualified name.");

        internal static readonly DiagnosticDescriptor REFL023TypeDoesNotImplementInterface = Descriptors.Create(
            id: "REFL023",
            title: "The type does not implement the interface.",
            messageFormat: "The type does not implement the interface.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not implement the interface.");

        internal static readonly DiagnosticDescriptor REFL024PreferNullOverEmptyArray = Descriptors.Create(
            id: "REFL024",
            title: "Prefer null over empty array.",
            messageFormat: "Prefer null over empty array.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer null over empty array.");

        internal static readonly DiagnosticDescriptor REFL025ArgumentsDoNotMatchParameters = Descriptors.Create(
            id: "REFL025",
            title: "Use correct arguments.",
            messageFormat: "Use correct arguments.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct arguments.");

        internal static readonly DiagnosticDescriptor REFL026NoDefaultConstructor = Descriptors.Create(
            id: "REFL026",
            title: "No parameterless constructor defined for this object.",
            messageFormat: "No parameterless constructor defined for {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "No parameterless constructor defined for this object.");

        internal static readonly DiagnosticDescriptor REFL027PreferEmptyTypes = Descriptors.Create(
            id: "REFL027",
            title: "Prefer Type.EmptyTypes.",
            messageFormat: "Prefer Type.EmptyTypes.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer Type.EmptyTypes.");

        internal static readonly DiagnosticDescriptor REFL028CastReturnValueToCorrectType = Descriptors.Create(
            id: "REFL028",
            title: "Cast return value to correct type.",
            messageFormat: "Cast return value to {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast return value to correct type.");

        internal static readonly DiagnosticDescriptor REFL029MissingTypes = Descriptors.Create(
            id: "REFL029",
            title: "Specify types in case an overload is added in the future.",
            messageFormat: "Specify types in case an overload is added in the future.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Specify types in case an overload is added in the future.");

        internal static readonly DiagnosticDescriptor REFL030UseCorrectObj = Descriptors.Create(
            id: "REFL030",
            title: "Use correct obj parameter.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct obj parameter.");

        internal static readonly DiagnosticDescriptor REFL031UseCorrectGenericArguments = Descriptors.Create(
            id: "REFL031",
            title: "Use generic arguments that satisfies the type parameters.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use generic arguments that satisfies the type parameters.");

        internal static readonly DiagnosticDescriptor REFL032DependencyMustExist = Descriptors.Create(
            id: "REFL032",
            title: "The dependency does not exist.",
            messageFormat: "The dependency does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The dependency does not exist.");

        internal static readonly DiagnosticDescriptor REFL033UseSameTypeAsParameter = Descriptors.Create(
            id: "REFL033",
            title: "Use the same type as the parameter.",
            messageFormat: "Use the same type as the parameter. Expected: {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the same type as the parameter.");

        internal static readonly DiagnosticDescriptor REFL034DoNotMakeGeneric = Descriptors.Create(
            id: "REFL034",
            title: "Don't call MakeGeneric when not a generic definition.",
            messageFormat: "{0}",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't call MakeGeneric when not a generic definition.");

        internal static readonly DiagnosticDescriptor REFL035DoNotInvokeGenericDefinition = Descriptors.Create(
            id: "REFL035",
            title: "Don't call Invoke on a generic definition.",
            messageFormat: "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't call Invoke on a generic definition.");

        internal static readonly DiagnosticDescriptor REFL036CheckNull = Descriptors.Create(
            id: "REFL036",
            title: "Pass 'throwOnError: true' or check if null.",
            messageFormat: "Pass 'throwOnError: true' or check if null.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Pass 'throwOnError: true' or check if null.");

        internal static readonly DiagnosticDescriptor REFL037TypeDoesNotExits = Descriptors.Create(
            id: "REFL037",
            title: "The type does not exist.",
            messageFormat: "The type does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The type does not exist.");

        internal static readonly DiagnosticDescriptor REFL038PreferRunClassConstructor = Descriptors.Create(
            id: "REFL038",
            title: "Prefer RuntimeHelpers.RunClassConstructor.",
            messageFormat: "Prefer RuntimeHelpers.RunClassConstructor.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().");

        internal static readonly DiagnosticDescriptor REFL039PreferTypeof = Descriptors.Create(
            id: "REFL039",
            title: "Prefer typeof(...) over instance.GetType when the type is sealed.",
            messageFormat: "Prefer typeof({0}) over instance.GetType when the type is sealed.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer typeof(...) over instance.GetType when the type is sealed.");

        internal static readonly DiagnosticDescriptor REFL040PreferIsInstanceOfType = Descriptors.Create(
            id: "REFL040",
            title: "Prefer type.IsInstanceOfType(...).",
            messageFormat: "Prefer {0}.IsInstanceOfType({1}).",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer type.IsInstanceOfType(...).");

        internal static readonly DiagnosticDescriptor REFL041CreateDelegateType = Descriptors.Create(
            id: "REFL041",
            title: "Delegate type is not matching.",
            messageFormat: "Delegate type is not matching expected {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Delegate type is not matching.");

        internal static readonly DiagnosticDescriptor REFL042FirstArgumentIsReferenceType = Descriptors.Create(
            id: "REFL042",
            title: "First argument must be reference type.",
            messageFormat: "First argument must be reference type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must be reference type.");

        internal static readonly DiagnosticDescriptor REFL043FirstArgumentType = Descriptors.Create(
            id: "REFL043",
            title: "First argument must match type.",
            messageFormat: "First argument must be of type {0}.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "First argument must match type.");

        internal static readonly DiagnosticDescriptor REFL044ExpectedAttributeType = Descriptors.Create(
            id: "REFL044",
            title: "Expected attribute type.",
            messageFormat: "Expected attribute type.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Expected attribute type.");

        internal static readonly DiagnosticDescriptor REFL045InsufficientFlags = Descriptors.Create(
            id: "REFL045",
            title: "These flags are insufficient to match any members.",
            messageFormat: "These flags are insufficient to match any members.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "These flags are insufficient to match any members.");

        internal static readonly DiagnosticDescriptor REFL046DefaultMemberMustExist = Descriptors.Create(
            id: "REFL046",
            title: "The specified default member does not exist.",
            messageFormat: "The specified default member does not exist.",
            category: AnalyzerCategory.SystemReflection,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The specified default member does not exist, or is not a valid target for Type.InvokeMember.");

        private static DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            bool isEnabledByDefault,
            string description,
            params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: description,
                helpLinkUri: $"https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/{id}.md",
                customTags: customTags);
        }
    }
}
