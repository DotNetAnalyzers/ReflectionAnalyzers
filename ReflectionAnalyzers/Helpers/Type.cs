namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out ITypeSymbol result, out Optional<ExpressionSyntax> source)
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

        private static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, PooledSet<ExpressionSyntax> visited, out ITypeSymbol result, out Optional<ExpressionSyntax> source)
        {
            source = default(Optional<ExpressionSyntax>);
            result = null;
            switch (expression)
            {
                case IdentifierNameSyntax identifierName when context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out ILocalSymbol local):
                    using (visited = visited.IncrementUsage())
                    {
                        return AssignedValueWalker.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                               visited.Add(assignedValue) &&
                               TryGet(assignedValue, context, visited, out result, out source);
                    }

                case TypeOfExpressionSyntax typeOf:
                    source = new Optional<ExpressionSyntax>(typeOf);
                    return context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out result);
                case InvocationExpressionSyntax getType when getType.TryGetMethodName(out var name) &&
                                                 name == "GetType" &&
                                                 getType.ArgumentList is ArgumentListSyntax args:
                    if (args.Arguments.Count == 0)
                    {
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess:
                                source = new Optional<ExpressionSyntax>(getType);
                                return context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out result);
                            case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType):
                                source = new Optional<ExpressionSyntax>(getType);
                                return context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out result);
                        }
                    }
                    else if (args.Arguments.TrySingle(out var arg) &&
                             arg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var typeName) &&
                             getType.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out _))
                    {
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess when context.SemanticModel.TryGetType(typeAccess.Expression, context.CancellationToken, out var typeInAssembly):
                                source = new Optional<ExpressionSyntax>(getType);
                                result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                                return result != null;
                            case IdentifierNameSyntax _ when expression.TryFirstAncestor(out TypeDeclarationSyntax containingType) &&
                                                             context.SemanticModel.TryGetSymbol(containingType, context.CancellationToken, out var typeInAssembly):
                                source = new Optional<ExpressionSyntax>(getType);
                                result = typeInAssembly.ContainingAssembly.GetTypeByMetadataName(typeName);
                                return result != null;
                        }
                    }

                    break;
            }

            return false;
        }
    }
}
