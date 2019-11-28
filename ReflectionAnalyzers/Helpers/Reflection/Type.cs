namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static string ToString(this ITypeSymbol type, SyntaxNodeAnalysisContext context) => ToString(type, context.SemanticModel, context.Node.SpanStart);

        internal static string ToString(this ITypeSymbol type, SemanticModel semanticModel, int position)
        {
            if (semanticModel.IsAccessible(position, type))
            {
                return type.ToMinimalDisplayString(semanticModel, position);
            }

            return ToString(type.BaseType, semanticModel, position);
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            return TryGet(expression, semanticModel, cancellationToken, null, out result, out source);
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
            if (type == null ||
                type.TypeKind == TypeKind.Interface ||
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
                nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name))
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
                nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name))
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

        private static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<ExpressionSyntax>? visited, [NotNullWhen(true)] out ITypeSymbol? result, [NotNullWhen(true)] out ExpressionSyntax? source)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "ReturnType" } } } memberAccess
                    when GetX.TryMatchGetMethod(invocation, semanticModel, cancellationToken, out var reflectedMember, out _, out _, out _) &&
                         reflectedMember.Match == FilterMatch.Single &&
                         reflectedMember.Symbol is IMethodSymbol method:
                    source = memberAccess;
                    result = method.ReturnType;
                    return true;
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "FieldType" } } } memberAccess
                    when GetX.TryMatchGetField(invocation, semanticModel, cancellationToken, out var reflectedMember, out _, out _) &&
                         reflectedMember.Match == FilterMatch.Single &&
                         reflectedMember.Symbol is IFieldSymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation, Name: { Identifier: { ValueText: "PropertyType" } } } memberAccess
                    when GetX.TryMatchGetProperty(invocation, semanticModel, cancellationToken, out var reflectedMember, out _, out _, out _) &&
                         reflectedMember.Match == FilterMatch.Single &&
                         reflectedMember.Symbol is IPropertySymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case TypeOfExpressionSyntax typeOf:
                    source = typeOf;
                    return semanticModel.TryGetType(typeOf.Type, cancellationToken, out result);
                case InvocationExpressionSyntax { ArgumentList: ArgumentListSyntax { Arguments: { Count: 0 } } } invocation
                    when invocation.TryGetMethodName(out var name) &&
                         name == "GetType":
                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax typeAccess:
                            source = invocation;
                            if (semanticModel.TryGetType(typeAccess.Expression, cancellationToken, out result))
                            {
                                if (result is INamedTypeSymbol namedType &&
                                    namedType.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
                                {
                                    result = namedType.TypeArguments[0];
                                }

                                return true;
                            }

                            return false;
                        case IdentifierNameSyntax _
                            when expression.TryFirstAncestor(out TypeDeclarationSyntax? containingType):
                            source = invocation;
                            return semanticModel.TryGetSymbol(containingType, cancellationToken, out result);
                        case MemberBindingExpressionSyntax { Parent: { Parent: ConditionalAccessExpressionSyntax { Expression: { } } conditionalAccess } }:
                            source = invocation;
                            return semanticModel.TryGetType(conditionalAccess.Expression, cancellationToken, out result);
                    }

                    break;
                case InvocationExpressionSyntax candidate
                    when TryMatchTypeGetType(candidate, semanticModel, cancellationToken, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = semanticModel.Compilation.GetTypeByMetadataName(typeName, ignoreCase.Value);
                    return result != null;
                case InvocationExpressionSyntax candidate
                    when TryMatchAssemblyGetType(candidate, semanticModel, cancellationToken, out var typeName, out var ignoreCase):
                    source = candidate;
                    result = Assembly.TryGet(candidate.Expression, semanticModel, cancellationToken, out var assembly)
                        ? assembly.GetTypeByMetadataName(typeName, ignoreCase.Value)
                        : null;
                    return result != null;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when invocation.TryGetTarget(KnownSymbol.Type.GetGenericTypeDefinition, semanticModel, cancellationToken, out _) &&
                         TryGet(memberAccess.Expression, semanticModel, cancellationToken, visited, out var definingType, out _) &&
                         definingType is INamedTypeSymbol namedType:
                    source = invocation;
                    result = namedType.ConstructedFrom;
                    return true;

                case InvocationExpressionSyntax invocation
                    when GetX.TryMatchGetNestedType(invocation, semanticModel, cancellationToken, out var reflectedMember, out _, out _):
                    source = invocation;
                    result = reflectedMember.Symbol as ITypeSymbol;
                    return result != null && reflectedMember.Match == FilterMatch.Single;
                case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
                    when invocation.TryGetTarget(KnownSymbol.Type.MakeGenericType, semanticModel, cancellationToken, out _) &&
                         TypeArguments.TryCreate(invocation, semanticModel, cancellationToken, out var typeArguments) &&
                         typeArguments.TryGetArgumentsTypes(semanticModel, cancellationToken, out var types):
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                    {
                        source = invocation;
                        if (visited.Add(invocation) &&
                            TryGet(memberAccess.Expression, semanticModel, cancellationToken, visited, out var definition, out _) &&
                            definition is INamedTypeSymbol namedType &&
                            ReferenceEquals(namedType, namedType.ConstructedFrom) &&
                            namedType.Arity == types.Length)
                        {
                            result = namedType.Construct(types);
                            return result != null;
                        }

                        result = null;
                        return false;
                    }

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    source = conditionalAccess;
                    return TryGet(conditionalAccess.WhenNotNull, semanticModel, cancellationToken, out result, out _);
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local))
            {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                {
                    source = null;
                    result = null;
                    return AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                           visited.Add(assignedValue) &&
                           TryGet(assignedValue, semanticModel, cancellationToken, visited, out result, out source);
                }
            }

            source = null;
            result = null;
            return false;
        }
    }
}
