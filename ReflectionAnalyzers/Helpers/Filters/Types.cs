namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct Types
    {
        internal static readonly Types Any = new Types(null, ImmutableArray<ITypeSymbol>.Empty);
        internal readonly ArgumentSyntax Argument;
        internal readonly ImmutableArray<ITypeSymbol> Values;

        public Types(ArgumentSyntax argument, ImmutableArray<ITypeSymbol> values)
        {
            this.Argument = argument;
            this.Values = values;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out Types types)
        {
            if (TryGetTypesArgument(invocation, getX, out var typesArg))
            {
                if (Array.TryGetTypes(typesArg.Expression, context, out var typeArray))
                {
                    types = new Types(typesArg, typeArray);
                    return true;
                }

                types = default(Types);
                return false;
            }

            types = Any;
            return true;
        }

        internal bool Matches(ImmutableArray<IParameterSymbol> parameters)
        {
            if (parameters.Length != this.Values.Length)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!this.Values[i].Is(parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetTypesArgument(InvocationExpressionSyntax invocation, IMethodSymbol getX, out ArgumentSyntax argument)
        {
            argument = null;
            return getX.TryFindParameter("types", out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument);
        }
    }
}
