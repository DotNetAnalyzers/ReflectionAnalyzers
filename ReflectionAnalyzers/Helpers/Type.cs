namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Type
    {
        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out ITypeSymbol result, out Optional<ExpressionSyntax> source)
        {
            return TryGet(expression, context, null, out result, out source);
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
