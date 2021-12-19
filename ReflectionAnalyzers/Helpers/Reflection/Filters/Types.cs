namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Argument}")]
    internal readonly struct Types
    {
        internal static readonly Types Any = new(null, ImmutableArray<ExpressionSyntax>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        internal readonly ArgumentSyntax? Argument;
        internal readonly ImmutableArray<ExpressionSyntax> Expressions;
        internal readonly ImmutableArray<ITypeSymbol> Symbols;

        internal Types(ArgumentSyntax? argument, ImmutableArray<ExpressionSyntax> expressions, ImmutableArray<ITypeSymbol> symbols)
        {
            this.Argument = argument;
            this.Expressions = expressions;
            this.Symbols = symbols;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SemanticModel semanticModel, CancellationToken cancellationToken, out Types types)
        {
            if (TryGetTypesArgument(invocation, getX, out var typesArg))
            {
                if (Array.TryGetValues(typesArg.Expression, semanticModel, cancellationToken, out var expressions) &&
                    Array.TryGetTypes(typesArg.Expression, semanticModel, cancellationToken, out var symbols))
                {
                    types = new Types(typesArg, expressions, symbols);
                    return true;
                }

                types = default;
                return false;
            }

            types = Any;
            return true;
        }

        internal static bool TryGetTypesArrayText(ImmutableArray<IParameterSymbol> parameters, SemanticModel semanticModel, int position, [NotNullWhen(true)] out string? typesArrayText)
        {
            if (parameters.IsEmpty)
            {
                typesArrayText = "Type.EmptyTypes";
                return true;
            }

            var builder = StringBuilderPool.Borrow().Append("new[] { ");
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (!semanticModel.IsAccessible(position, parameter.Type))
                {
                    _ = builder.Return();
                    typesArrayText = null;
                    return false;
                }

                if (i != 0)
                {
                    _ = builder.Append(", ");
                }

                _ = builder.Append("typeof(")
                           .Append(parameter.Type.ToString(semanticModel, position))
                           .Append(")");
            }

            typesArrayText = builder.Append(" }").Return();
            return true;
        }

        internal bool Matches(ImmutableArray<IParameterSymbol> parameters, Compilation compilation)
        {
            if (parameters.Length != this.Expressions.Length)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!this.Symbols[i].IsAssignableTo(parameters[i].Type, compilation))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool TryMostSpecific(ISymbol? x, ISymbol? y, Compilation compilation, [NotNullWhen(true)] out ISymbol? unique)
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

            if (this.Argument is null ||
                this.Expressions.IsEmpty)
            {
                unique = null;
                return false;
            }

            return this.TryMostSpecific(Parameters(x), Parameters(y), compilation, out unique);

            static bool ByNull(ISymbol? first, ISymbol? other, [NotNullWhen(true)] out ISymbol? result)
            {
                if (first is null &&
                    other is not null)
                {
                    result = other;
                    return true;
                }

                result = null;
                return false;
            }

            static ImmutableArray<IParameterSymbol> Parameters(ISymbol symbol)
            {
                return symbol switch
                {
                    IMethodSymbol method => method.Parameters,
                    IPropertySymbol property => property.Parameters,
                    _ => ImmutableArray<IParameterSymbol>.Empty,
                };
            }
        }

        private static bool TryGetTypesArgument(InvocationExpressionSyntax invocation, IMethodSymbol getX, [NotNullWhen(true)] out ArgumentSyntax? argument)
        {
            argument = null;
            return getX.TryFindParameter("types", out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument);
        }

        private bool TryMostSpecific(ImmutableArray<IParameterSymbol> x, ImmutableArray<IParameterSymbol> y, Compilation compilation, [NotNullWhen(true)] out ISymbol? unique)
        {
            if (this.Argument is null ||
                x.IsDefaultOrEmpty ||
                y.IsDefaultOrEmpty)
            {
                unique = null;
                return false;
            }

            if (TryExact(this.Symbols, x, out unique) ||
                TryExact(this.Symbols, y, out unique))
            {
                return true;
            }

            if (this.Matches(x, compilation) &&
                this.Matches(y, compilation))
            {
                var sum = 0;
                for (var i = 0; i < this.Symbols.Length; i++)
                {
                    sum += ByIndex(i, this.Symbols);
                }

                if (sum == 0)
                {
                    unique = null;
                    return false;
                }

                unique = sum < 0 ? x[0].ContainingSymbol : y[0].ContainingSymbol;
                return true;
            }

            if (this.Matches(x, compilation))
            {
                unique = x[0].ContainingSymbol;
                return true;
            }

            if (this.Matches(y, compilation))
            {
                unique = y[0].ContainingSymbol;
                return true;
            }

            unique = null;
            return false;

            static bool TryExact(ImmutableArray<ITypeSymbol> symbols, ImmutableArray<IParameterSymbol> parameters, [NotNullWhen(true)] out ISymbol? result)
            {
                if (parameters.Length != symbols.Length)
                {
                    result = null;
                    return false;
                }

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (!TypeSymbolComparer.Equal(symbols[i], parameters[i].Type))
                    {
                        result = null;
                        return false;
                    }
                }

                result = parameters[0].ContainingSymbol;
                return true;
            }

            int ByIndex(int index, ImmutableArray<ITypeSymbol> symbols)
            {
                var xt = x[index].Type;
                var yt = y[index].Type;
                if (TypeSymbolComparer.Equal(xt, yt))
                {
                    return 0;
                }

                if (TypeSymbolComparer.Equal(xt, symbols[index]))
                {
                    return -1;
                }

                if (TypeSymbolComparer.Equal(yt, symbols[index]))
                {
                    return 1;
                }

                if (xt == KnownSymbol.Object)
                {
                    return 1;
                }

                if (yt == KnownSymbol.Object)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
}
