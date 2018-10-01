namespace ReflectionAnalyzers
{
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out ITypeSymbol result, out ExpressionSyntax source)
        {
            return TryGet(expression, context, null, out result, out source);
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

        internal static bool SatisfiesConstraints(ITypeSymbol type, ITypeParameterSymbol typeParameter, Compilation compilation)
        {
            if (typeParameter.HasConstructorConstraint)
            {
                switch (type)
                {
                    case INamedTypeSymbol namedType when !namedType.Constructors.TryFirst(x => x.DeclaredAccessibility == Accessibility.Public && x.Parameters.Length == 0, out _):
                    case ITypeParameterSymbol parameter when !parameter.HasConstructorConstraint:
                        return false;
                }
            }

            if (typeParameter.HasReferenceTypeConstraint)
            {
                switch (type)
                {
                    case INamedTypeSymbol namedType when !namedType.IsReferenceType:
                    case ITypeParameterSymbol parameter when !parameter.HasReferenceTypeConstraint:
                        return false;
                }
            }

            if (typeParameter.HasValueTypeConstraint)
            {
                switch (type)
                {
                    case INamedTypeSymbol namedType when !namedType.IsValueType:
                    case ITypeParameterSymbol parameter when !parameter.HasValueTypeConstraint:
                        return false;
                }
            }

            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                switch (constraintType)
                {
                    case ITypeParameterSymbol parameter when !SatisfiesConstraints(type, parameter, compilation):
                        return false;
                    case INamedTypeSymbol namedType:
                        var conversion = compilation.ClassifyConversion(type, namedType);
                        if (!conversion.Exists ||
                            conversion.IsExplicit)
                        {
                            return false;
                        }

                        break;
                }
            }

            return true;
        }

        private static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, PooledSet<ExpressionSyntax> visited, out ITypeSymbol result, out ExpressionSyntax source)
        {
            switch (expression)
            {
                case IdentifierNameSyntax identifierName when context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out ILocalSymbol local):
                    using (visited = visited.IncrementUsage())
                    {
                        source = null;
                        result = null;
                        return AssignedValueWalker.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                               visited.Add(assignedValue) &&
                               TryGet(assignedValue, context, visited, out result, out source);
                    }

                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "ReturnType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetMethod(invocation, context, out var reflectedMember, out _, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IMethodSymbol method:
                    source = memberAccess;
                    result = method.ReturnType;
                    return true;
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "FieldType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetField(invocation, context, out var reflectedMember, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IFieldSymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name.Identifier.ValueText == "PropertyType" &&
                                                                    memberAccess.Expression is InvocationExpressionSyntax invocation &&
                                                                    GetX.TryMatchGetProperty(invocation, context, out var reflectedMember, out _, out _) &&
                                                                    reflectedMember.Match == FilterMatch.Single &&
                                                                    reflectedMember.Symbol is IPropertySymbol field:
                    source = memberAccess;
                    result = field.Type;
                    return true;
                case TypeOfExpressionSyntax typeOf:
                    source = typeOf;
                    return context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out result);
                case InvocationExpressionSyntax invocation when invocation.ArgumentList is ArgumentListSyntax args &&
                                                                args.Arguments.Count == 0 &&
                                                                invocation.TryGetMethodName(out var name) &&
                                                                name == "GetType":
                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax typeAccess:
                            source = invocation;
                            return context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out result);
                        case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType):
                            source = invocation;
                            return context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out result);
                    }

                    break;
                case InvocationExpressionSyntax invocation when invocation.ArgumentList is ArgumentListSyntax args &&
                                                                args.Arguments.TrySingle(out var arg) &&
                                                                arg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var typeName) &&
                                                                invocation.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out _):

                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax typeAccess when context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out var typeInAssembly):
                            source = invocation;
                            result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                            return result != null;
                        case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType) &&
                                                         context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out var typeInAssembly):
                            source = invocation;
                            result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                            return result != null;
                    }

                    break;
                case InvocationExpressionSyntax invocation when GetX.TryMatchGetNestedType(invocation, context, out var reflectedMember, out _, out _):
                    source = invocation;
                    result = reflectedMember.Symbol as ITypeSymbol;
                    return result != null && reflectedMember.Match == FilterMatch.Single;
                case InvocationExpressionSyntax invocation when invocation.TryGetTarget(KnownSymbol.Type.MakeGenericType, context.SemanticModel, context.CancellationToken, out _) &&
                                                                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                                                TypeArguments.TryCreate(invocation, context, out var typeArguments) &&
                                                                Array.TryGetTypes(typeArguments, context, out var types):
                    using (visited = visited.IncrementUsage())
                    {
                        if (visited.Add(invocation) &&
                            TryGet(memberAccess.Expression, context, visited, out var definition, out _) &&
                            definition is INamedTypeSymbol namedType)
                        {
                            source = invocation;
                            result = namedType.Construct(types.ToArray());
                            return result != null;
                        }
                    }

                    break;
            }

            source = null;
            result = null;
            return false;
        }
    }
}
