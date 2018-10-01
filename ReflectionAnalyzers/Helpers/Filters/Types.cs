namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct Types
    {
        internal static readonly Types Any = new Types(null, ImmutableArray<ExpressionSyntax>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        internal readonly ArgumentSyntax Argument;
        internal readonly ImmutableArray<ExpressionSyntax> Expressions;
        internal readonly ImmutableArray<ITypeSymbol> Symbols;

        public Types(ArgumentSyntax argument, ImmutableArray<ExpressionSyntax> expressions, ImmutableArray<ITypeSymbol> symbols)
        {
            this.Argument = argument;
            this.Expressions = expressions;
            this.Symbols = symbols;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out Types types)
        {
            if (TryGetTypesArgument(invocation, getX, out var typesArg))
            {
                if (Array.TryGetValues(typesArg.Expression, context, out var expressions) &&
                    Array.TryGetTypes(typesArg.Expression, context, out var symbols))
                {
                    types = new Types(typesArg, expressions, symbols);
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
            if (parameters.Length != this.Expressions.Length)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!this.Symbols[i].Is(parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool TryMostSpecific(ISymbol x, ISymbol y, out ISymbol unique)
        {
            if (x is null &&
                y is null)
            {
                unique = null;
                return false;
            }

            if (ByNull(x, y, out unique) ||
                ByNull(y, x, out unique))
            {
                return true;
            }

            return this.TryMostSpecific(x as IMethodSymbol, y as IMethodSymbol, out unique);

            bool ByNull(ISymbol first, ISymbol other, out ISymbol result)
            {
                if (first is null &&
                    !(other is null))
                {
                    result = other;
                    return true;
                }

                result = null;
                return false;
            }
        }

        private static bool TryGetTypesArgument(InvocationExpressionSyntax invocation, IMethodSymbol getX, out ArgumentSyntax argument)
        {
            argument = null;
            return getX.TryFindParameter("types", out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument);
        }

        private bool TryMostSpecific(IMethodSymbol x, IMethodSymbol y, out ISymbol unique)
        {
            if (this.Argument is null ||
                x is null ||
                y is null)
            {
                unique = null;
                return false;
            }

            if (TryExact(this.Symbols, x, out unique) ||
                TryExact(this.Symbols, y, out unique))
            {
                return true;
            }

            if (this.Matches(x.Parameters) &&
                this.Matches(y.Parameters))
            {
                for (var i = 0; i < this.Symbols.Length; i++)
                {
                    if (TryIndex(i, this.Symbols, out unique))
                    {
                        return true;
                    }
                }

                unique = null;
                return false;
            }

            if (this.Matches(x.Parameters))
            {
                unique = x;
                return true;
            }

            if (this.Matches(y.Parameters))
            {
                unique = y;
                return true;
            }

            unique = null;
            return false;

            bool TryExact(ImmutableArray<ITypeSymbol> symbols, IMethodSymbol method, out ISymbol result)
            {
                if (method.Parameters.Length != symbols.Length)
                {
                    result = null;
                    return false;
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    if (!symbols[i].Equals(method.Parameters[i].Type))
                    {
                        result = null;
                        return false;
                    }
                }

                result = method;
                return true;
            }

            bool TryIndex(int index, ImmutableArray<ITypeSymbol> symbols, out ISymbol result)
            {
                var xt = x.Parameters[index].Type;
                var yt = y.Parameters[index].Type;
                if (xt.Equals(yt))
                {
                    result = null;
                    return false;
                }

                if (xt.Equals(symbols[index]))
                {
                    result = x;
                    return true;
                }

                if (yt.Equals(symbols[index]))
                {
                    result = y;
                    return true;
                }

                if (xt == KnownSymbol.Object)
                {
                    result = y;
                    return true;
                }

                if (yt == KnownSymbol.Object)
                {
                    result = x;
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
