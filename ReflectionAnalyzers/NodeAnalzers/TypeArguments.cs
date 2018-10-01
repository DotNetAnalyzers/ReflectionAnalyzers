namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct TypeArguments
    {
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        internal readonly ImmutableArray<ITypeParameterSymbol> Parameters;
        internal readonly ImmutableArray<ExpressionSyntax> Arguments;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

        public TypeArguments(ImmutableArray<ITypeParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> arguments)
        {
            this.Parameters = parameters;
            this.Arguments = arguments;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out TypeArguments typeArguments)
        {
            if (invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                (TryGetTypeParameters(invocation, context, out var parameters) ||
                 TryGetMethodParameters(invocation, context, out parameters)))
            {
                if (argumentList.Arguments.TrySingle(out var argument) &&
                    Array.TryGetValues(argument.Expression, context, out var arrayExpressions))
                {
                    typeArguments = new TypeArguments(parameters, arrayExpressions);
                    return true;
                }

                if (!IsUnknownArray())
                {
                    typeArguments = new TypeArguments(parameters, ArgumentsExpressions());
                    return true;
                }
            }

            typeArguments = default(TypeArguments);
            return false;

            ImmutableArray<ExpressionSyntax> ArgumentsExpressions()
            {
                var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>(argumentList.Arguments.Count);
                foreach (var arg in argumentList.Arguments)
                {
                    builder.Add(arg.Expression);
                }

                return builder.ToImmutable();
            }

            bool IsUnknownArray()
            {
                return argumentList.Arguments.TrySingle(out var single) &&
                       single.Expression is IdentifierNameSyntax identifierName &&
                       context.SemanticModel.TryGetType(identifierName, context.CancellationToken, out var type) &&
                       type is IArrayTypeSymbol;
            }
        }

        internal bool TryFindMisMatch(SyntaxNodeAnalysisContext context, out ExpressionSyntax argument)
        {
            for (var i = 0; i < this.Arguments.Length; i++)
            {
                if (Type.TryGet(this.Arguments[i], context, out var type, out _) &&
                    !Type.SatisfiesConstraints(type, this.Parameters[i], context.Compilation))
                {
                    argument = this.Arguments[i];
                    return true;
                }
            }

            argument = null;
            return false;
        }

        private static bool TryGetTypeParameters(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ImmutableArray<ITypeParameterSymbol> parameters)
        {
            if (IsMakeGeneric(invocation, KnownSymbol.Type.MakeGenericType, context) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (Type.TryGet(memberAccess.Expression, context, out var type, out _))
                {
                    parameters = (type as INamedTypeSymbol)?.TypeParameters ?? ImmutableArray<ITypeParameterSymbol>.Empty;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetMethodParameters(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ImmutableArray<ITypeParameterSymbol> parameters)
        {
            if (IsMakeGeneric(invocation, KnownSymbol.MethodInfo.MakeGenericMethod, context) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (GetX.TryGetMethodInfo(memberAccess, context, out var methodInfo))
                {
                    parameters = methodInfo.TypeParameters;
                    return true;
                }
            }

            return false;
        }

        private static bool IsMakeGeneric(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context)
        {
            return invocation.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out IMethodSymbol makeGeneric) &&
                   makeGeneric.Parameters.TrySingle(out var parameter) &&
                   parameter.IsParams &&
                   parameter.Type is IArrayTypeSymbol arrayType &&
                   arrayType.ElementType == KnownSymbol.Type;
        }
    }
}
