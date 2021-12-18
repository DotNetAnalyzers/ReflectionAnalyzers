namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static string ToString(this ITypeSymbol type, SyntaxNodeAnalysisContext context) => ToString(type, context.SemanticModel, context.Node.SpanStart);

        internal static string ToString(this ITypeSymbol type, SemanticModel semanticModel, int position)
        {
            if (semanticModel.IsAccessible(position, type) ||
                type.BaseType is null)
            {
                return type.ToMinimalDisplayString(semanticModel, position);
            }

            return ToString(type.BaseType, semanticModel, position);
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            if (expression.TryFirstAncestor(out TypeDeclarationSyntax? containingType) &&
                semanticModel.TryGetNamedType(containingType, cancellationToken, out var type))
            {
                using var recursion = Recursion.Borrow(type, semanticModel, cancellationToken);
                return TryGet(expression, recursion, out result, out source);
            }

            result = null;
            source = null;
            return false;
        }

        internal static bool HasVisibleMembers(ITypeSymbol type, BindingFlags flags)
        {
            if (!flags.HasFlagFast(BindingFlags.NonPublic))
            {
                return true;
            }

            if (flags.HasFlagFast(BindingFlags.DeclaredOnly))
            {
                return HasVisibleNonPublicMembers(type, recursive: false);
            }

            if (!flags.HasFlagFast(BindingFlags.Instance) &&
                !flags.HasFlagFast(BindingFlags.FlattenHierarchy))
            {
                return HasVisibleNonPublicMembers(type, recursive: false);
            }

            return HasVisibleNonPublicMembers(type, recursive: true);
        }

        internal static bool HasVisibleNonPublicMembers(ITypeSymbol type, bool recursive)
        {
            if (type.TypeKind == TypeKind.Interface ||
                type.BaseType is null ||
                type == KnownSymbol.Object)
            {
                return true;
            }

            if (!type.Locations.TryFirst(x => x.IsInSource, out _))
            {
                return false;
            }

            return !recursive || HasVisibleNonPublicMembers(type.BaseType, recursive: true);
        }

        internal static bool IsCastToWrongType(InvocationExpressionSyntax invocation, ITypeSymbol expectedType, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            if (semanticModel.IsAccessible(invocation.SpanStart, expectedType))
            {
                switch (invocation.Parent)
                {
                    case CastExpressionSyntax castExpression
                        when semanticModel.TryGetType(castExpression.Type, cancellationToken, out var castType) &&
                             !expectedType.IsAssignableTo(castType, semanticModel.Compilation):
                        typeSyntax = castExpression.Type;
                        return true;
                }
            }

            typeSyntax = null;
            return false;
        }

        internal static bool TryMatchTypeGetType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out TypeNameArgument typeName, out ArgumentAndValue<bool> ignoreCase)
        {
            if (invocation.TryGetTarget(KnownSymbol.Type.GetType, semanticModel, cancellationToken, out var target) &&
                target.TryFindParameter("typeName", out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out var nameArg) &&
                nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name) &&
                name is { })
            {
                typeName = new TypeNameArgument(nameArg, name);
                switch (target.Parameters.Length)
                {
                    case 1:
                        ignoreCase = default;
                        return true;
                    case 2 when target.TryFindParameter("throwOnError", out _):
                        ignoreCase = default;
                        return true;
                    case 3 when target.TryFindParameter("throwOnError", out _) &&
                                target.TryFindParameter("ignoreCase", out var ignoreCaseParameter) &&
                                invocation.TryFindArgument(ignoreCaseParameter, out var ignoreCaseArg) &&
                                semanticModel.TryGetConstantValue(ignoreCaseArg.Expression, cancellationToken, out bool ignoreNameCase):
                        ignoreCase = new ArgumentAndValue<bool>(ignoreCaseArg, ignoreNameCase);
                        return true;
                }
            }

            typeName = default;
            ignoreCase = default;
            return false;
        }

        internal static bool TryMatchAssemblyGetType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out TypeNameArgument typeName, out ArgumentAndValue<bool> ignoreCase)
        {
            if (invocation.TryGetTarget(KnownSymbol.Assembly.GetType, semanticModel, cancellationToken, out var target) &&
                target.TryFindParameter("name", out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out var nameArg) &&
                nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name) &&
                name is { })
            {
                typeName = new TypeNameArgument(nameArg, name);
                switch (target.Parameters.Length)
                {
                    case 1:
                        ignoreCase = default;
                        return true;
                    case 2 when target.TryFindParameter("throwOnError", out _):
                        ignoreCase = default;
                        return true;
                    case 3 when target.TryFindParameter("throwOnError", out _) &&
                                target.TryFindParameter("ignoreCase", out var ignoreCaseParameter) &&
                                invocation.TryFindArgument(ignoreCaseParameter, out var ignoreCaseArg) &&
                                semanticModel.TryGetConstantValue(ignoreCaseArg.Expression, cancellationToken, out bool ignoreNameCase):
                        ignoreCase = new ArgumentAndValue<bool>(ignoreCaseArg, ignoreNameCase);
                        return true;
                }
            }

            typeName = default;
            ignoreCase = default;
            return false;
        }

        private static bool TryGet(ExpressionSyntax expression, Recursion recursion, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "ReturnType" } } } memberAccess
                    when GetMethod.Match(invocation, recursion.SemanticModel, recursion.CancellationToken) is { Single: { } single }:
                    source = memberAccess;
                    result = single.ReturnType;
                    return true;
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "FieldType" } } } memberAccess
                    when GetX.TryMatchGetField(invocation, recursion.SemanticModel, recursion.CancellationToken, out var reflectedMember, out _, out _) &&
                         reflectedMember.Match == FilterMatch.Single &&
                         reflectedMember.Symbol is IFieldSymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "PropertyType" } } } memberAccess
                    when GetX.TryMatchGetProperty(invocation, recursion.SemanticModel, recursion.CancellationToken, out var reflectedMember, out _, out _, out _) &&
                         reflectedMember.Match == FilterMatch.Single &&
                         reflectedMember.Symbol is IPropertySymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case TypeOfExpressionSyntax typeOf:
                    source = typeOf;
                    return recursion.SemanticModel.TryGetType(typeOf.Type, recursion.CancellationToken, out result);
                case InvocationExpressionSyntax { ArgumentList: { Arguments: { Count: 0 } } } invocation
                    when invocation.TryGetMethodName(out var name) &&
                         name == "GetType":
                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax typeAccess:
                            source = invocation;
                            if (recursion.SemanticModel.TryGetType(typeAccess.Expression, recursion.CancellationToken, out result))
                            {
                                if (result is INamedTypeSymbol { TypeArguments: { Length: 1 } typeArguments, ConstructedFrom: { SpecialType: SpecialType.System_Nullable_T } })
                                {
                                    result = typeArguments[0];
                                }

                                return true;
                            }

                            return false;
                        case IdentifierNameSyntax _
                            when expression.TryFirstAncestor(out TypeDeclarationSyntax? containingType):
                            source = invocation;
                            return recursion.SemanticModel.TryGetSymbol(containingType, recursion.CancellationToken, out result);
                        case MemberBindingExpressionSyntax { Parent: { Parent: ConditionalAccessExpressionSyntax { Expression: { } } conditionalAccess } }:
                            source = invocation;
                            return recursion.SemanticModel.TryGetType(conditionalAccess.Expression, recursion.CancellationToken, out result);
                    }

                    break;
                case InvocationExpressionSyntax candidate
                    when TryMatchTypeGetType(candidate, recursion.SemanticModel, recursion.CancellationToken, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = recursion.SemanticModel.Compilation.GetTypeByMetadataName(typeName, ignoreCase.Value);
                    return result != null;
                case InvocationExpressionSyntax candidate
                    when TryMatchAssemblyGetType(candidate, recursion.SemanticModel, recursion.CancellationToken, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = Assembly.Find(candidate.Expression, recursion.SemanticModel, recursion.CancellationToken) is { } assembly
                        ? assembly.GetTypeByMetadataName(typeName, ignoreCase.Value)
                        : null;
                    return result != null;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when invocation.TryGetTarget(KnownSymbol.Type.GetGenericTypeDefinition, recursion.SemanticModel, recursion.CancellationToken, out _) &&
                         TryGet(memberAccess.Expression, recursion, out var definingType, out _) &&
                         definingType is INamedTypeSymbol { ConstructedFrom: { } constructedFrom }:
                    source = invocation;
                    result = constructedFrom;
                    return true;

                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetNestedType(invocation, recursion.SemanticModel, recursion.CancellationToken, out var reflectedMember, out _, out _):
                    source = invocation;
                    result = reflectedMember.Symbol as ITypeSymbol;
                    return result != null && reflectedMember.Match == FilterMatch.Single;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when invocation.TryGetTarget(KnownSymbol.Type.MakeGenericType, recursion.SemanticModel, recursion.CancellationToken, out _) &&
                         TypeArguments.Find(invocation, recursion.SemanticModel, recursion.CancellationToken) is { } typeArguments &&
                         typeArguments.TryGetArgumentsTypes(recursion.SemanticModel, recursion.CancellationToken, out var types):
                    source = invocation;
                    if (TryGet(memberAccess.Expression, recursion, out var definition, out _) &&
                        definition is INamedTypeSymbol namedType &&
                        ReferenceEquals(namedType, namedType.ConstructedFrom) &&
                        namedType.Arity == types.Length)
                    {
                        result = namedType.Construct(types);
                        return true;
                    }

                    result = null;
                    return false;

                case ConditionalAccessExpressionSyntax { WhenNotNull: { } whenNotNull } conditionalAccess:
                    source = conditionalAccess;
                    return TryGet(whenNotNull, recursion, out result, out _);
                default:
                    switch (recursion.Target(expression))
                    {
                        case { Symbol: ILocalSymbol local }
                            when AssignedValue.FindSingle(local, recursion.SemanticModel, recursion.CancellationToken) is { } value:
                            return TryGet(value, recursion, out result, out source);
                        case { Symbol: IFieldSymbol field }
                            when AssignedValue.FindSingle(field, recursion.SemanticModel, recursion.CancellationToken) is { } value:
                            return TryGet(value, recursion, out result, out source);
                        case { Declaration: { } declaration, Symbol: IPropertySymbol property }
                            when !property.IsAutoProperty():
                            result = null;
                            source = null;
                            return ReturnValueWalker.TrySingle(declaration, out var returnValue) &&
                                   TryGet(returnValue, recursion, out result, out source);
                        case { Symbol: IPropertySymbol property }
                            when AssignedValue.FindSingle(property, recursion.SemanticModel, recursion.CancellationToken) is { } value:
                            return TryGet(value, recursion, out result, out source);
                        case { Declaration: { } declaration, Symbol: IMethodSymbol _ }:
                            result = null;
                            source = null;
                            return ReturnValueWalker.TrySingle(declaration, out returnValue) &&
                                   TryGet(returnValue, recursion, out result, out source);
                    }

                    break;
            }

            source = null;
            result = null;
            return false;
        }
    }
}
