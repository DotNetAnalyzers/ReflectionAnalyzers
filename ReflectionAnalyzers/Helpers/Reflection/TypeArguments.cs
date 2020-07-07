namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct TypeArguments
    {
        internal readonly ISymbol Symbol;
        internal readonly ImmutableArray<ITypeParameterSymbol> Parameters;
        internal readonly ImmutableArray<ExpressionSyntax> Arguments;

        internal TypeArguments(ISymbol symbol, ImmutableArray<ITypeParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> arguments)
        {
            this.Symbol = symbol;
            this.Parameters = parameters;
            this.Arguments = arguments;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out TypeArguments typeArguments)
        {
            if (invocation?.ArgumentList is { } argumentList &&
                (TryGetTypeParameters(invocation, semanticModel, cancellationToken, out var symbol, out var parameters) ||
                 TryGetMethodParameters(invocation, semanticModel, cancellationToken, out symbol, out parameters)))
            {
                if (argumentList.Arguments.TrySingle(out var argument) &&
                    Array.TryGetValues(argument.Expression, semanticModel, cancellationToken, out var arrayExpressions))
                {
                    typeArguments = new TypeArguments(symbol, parameters, arrayExpressions);
                    return true;
                }

                if (!IsUnknownArray())
                {
                    typeArguments = new TypeArguments(symbol, parameters, ArgumentsExpressions());
                    return true;

                    ImmutableArray<ExpressionSyntax> ArgumentsExpressions()
                    {
                        var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>(argumentList.Arguments.Count);
                        foreach (var arg in argumentList.Arguments)
                        {
                            builder.Add(arg.Expression);
                        }

                        return builder.ToImmutable();
                    }
                }
            }

            typeArguments = default;
            return false;

            bool IsUnknownArray()
            {
                if (argumentList.Arguments.TrySingle(out var single))
                {
                    return single.Expression switch
                    {
                        TypeOfExpressionSyntax _ => false,
                        _ => true,
                    };
                }

                return false;
            }
        }

        internal bool TryFindConstraintViolation(SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? argument, [NotNullWhen(true)] out ITypeParameterSymbol? parameter)
        {
            for (var i = 0; i < this.Parameters.Length; i++)
            {
                if (!this.SatisfiesConstraints(this.Arguments[i], this.Parameters[i], semanticModel, cancellationToken))
                {
                    argument = this.Arguments[i];
                    parameter = this.Parameters[i];
                    return true;
                }
            }

            argument = null;
            parameter = null;
            return false;
        }

        internal bool TryGetArgumentsTypes(SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol[] types)
        {
            if (this.Arguments.IsEmpty)
            {
                types = System.Array.Empty<ITypeSymbol>();
                return false;
            }

            types = new ITypeSymbol[this.Arguments.Length];
            for (var i = 0; i < this.Arguments.Length; i++)
            {
                var argument = this.Arguments[i];
                if (Type.TryGet(argument, semanticModel, cancellationToken, out var type, out _))
                {
                    types[i] = type;
                }
                else
                {
                    types = System.Array.Empty<ITypeSymbol>();
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetTypeParameters(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ISymbol? symbol, out ImmutableArray<ITypeParameterSymbol> parameters)
        {
            if (IsMakeGeneric(invocation, KnownSymbol.Type.MakeGenericType, semanticModel, cancellationToken) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (Type.TryGet(memberAccess.Expression, semanticModel, cancellationToken, out var type, out _) &&
                    type is INamedTypeSymbol namedType)
                {
                    symbol = type;
                    parameters = namedType.TypeParameters;

                    while (type.ContainingType is { } containingType)
                    {
                        parameters = parameters.InsertRange(0, containingType.TypeParameters);
                        type = containingType;
                    }

                    return true;
                }
            }

            symbol = null;
            return false;
        }

        private static bool TryGetMethodParameters(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ISymbol? symbol, out ImmutableArray<ITypeParameterSymbol> parameters)
        {
            if (IsMakeGeneric(invocation, KnownSymbol.MethodInfo.MakeGenericMethod, semanticModel, cancellationToken) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (GetX.TryGetMethodInfo(memberAccess, semanticModel, cancellationToken, out var method))
                {
                    symbol = method;
                    parameters = method.TypeParameters;
                    return true;
                }
            }

            symbol = null;
            return false;
        }

        private static bool IsMakeGeneric(InvocationExpressionSyntax invocation, QualifiedMethod expected, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return invocation.TryGetTarget(expected, semanticModel, cancellationToken, out var makeGeneric) &&
                   makeGeneric.Parameters.TrySingle(out var parameter) &&
                   parameter.IsParams &&
                   parameter.Type is IArrayTypeSymbol arrayType &&
                   arrayType.ElementType == KnownSymbol.Type;
        }

        private bool SatisfiesConstraints(ExpressionSyntax expression, ITypeParameterSymbol typeParameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (Type.TryGet(expression, semanticModel, cancellationToken, out var type, out _))
            {
                if (typeParameter.HasConstructorConstraint)
                {
                    switch (type)
                    {
                        case INamedTypeSymbol namedType
                            when !namedType.Constructors.TryFirst(x => x.DeclaredAccessibility == Accessibility.Public && x.Parameters.IsEmpty, out _):
                        case ITypeParameterSymbol { HasConstructorConstraint: false }:
                            return false;
                    }
                }

                if (typeParameter.HasReferenceTypeConstraint)
                {
                    switch (type)
                    {
                        case INamedTypeSymbol { IsReferenceType: false }:
                        case ITypeParameterSymbol parameter
                            when parameter.HasValueTypeConstraint ||
                                 IsValueTypeContext(expression, parameter):
                            return false;
                    }
                }

                if (typeParameter.HasValueTypeConstraint)
                {
                    switch (type)
                    {
                        case INamedTypeSymbol namedType
                            when !namedType.IsValueType ||
                                 namedType == KnownSymbol.NullableOfT:
                        case ITypeParameterSymbol parameter
                            when parameter.HasReferenceTypeConstraint ||
                                 !IsValueTypeContext(expression, parameter):
                            return false;
                    }
                }

                foreach (var constraintType in typeParameter.ConstraintTypes)
                {
                    switch (constraintType)
                    {
                        case ITypeParameterSymbol parameter when this.TryFindArgumentType(parameter, semanticModel, cancellationToken, out var argumentType):
                            if (!IsAssignableTo(type, argumentType))
                            {
                                return false;
                            }

                            break;
                        case INamedTypeSymbol namedType:
                            if (!IsAssignableTo(type, namedType))
                            {
                                if (namedType.IsGenericType)
                                {
                                    if (namedType.TypeArguments.All(x => x.TypeKind != TypeKind.TypeParameter))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            break;
                    }
                }
            }

            return true;

            static bool IsValueTypeContext(SyntaxNode node, ITypeParameterSymbol candidate)
            {
                if (node.TryFirstAncestor(out ConditionalExpressionSyntax? ternary))
                {
                    if (ternary.WhenTrue.Contains(node) &&
                        TryGetEffectivelyValueType(ternary.Condition, out var result))
                    {
                        return result;
                    }
                    else if (ternary.WhenFalse.Contains(node) &&
                             TryGetEffectivelyValueType(ternary.Condition, out result))
                    {
                        return !result;
                    }

                    return IsValueTypeContext(ternary, candidate);
                }

                if (node.TryFirstAncestor(out IfStatementSyntax? ifStatement))
                {
                    if (ifStatement.Statement.Contains(node) &&
                        TryGetEffectivelyValueType(ifStatement.Condition, out var result))
                    {
                        return result;
                    }
                    else if (ifStatement.Else?.Contains(node) == true &&
                             TryGetEffectivelyValueType(ifStatement.Condition, out result))
                    {
                        return !result;
                    }

                    return IsValueTypeContext(ifStatement, candidate);
                }

                return false;

                bool TryGetEffectivelyValueType(ExpressionSyntax condition, out bool result)
                {
                    switch (condition)
                    {
                        case MemberAccessExpressionSyntax { Expression: TypeOfExpressionSyntax { Type: IdentifierNameSyntax identifierName } } memberAccess
                            when memberAccess.Name.Identifier.Text == "IsValueType" &&
                                 identifierName.Identifier.Text == candidate.Name:
                            result = true;
                            return true;
                        case PrefixUnaryExpressionSyntax prefixUnary
                            when prefixUnary.IsKind(SyntaxKind.LogicalNotExpression):
                            return !TryGetEffectivelyValueType(prefixUnary.Operand, out result);
                        case BinaryExpressionSyntax binary
                            when binary.IsKind(SyntaxKind.LogicalAndExpression):
                            return TryGetEffectivelyValueType(binary.Left, out result) ||
                                   TryGetEffectivelyValueType(binary.Right, out result);
                    }

                    result = false;
                    return false;
                }
            }

            bool IsAssignableTo(ITypeSymbol source, ITypeSymbol destination)
            {
                var conversion = semanticModel.Compilation.ClassifyConversion(source, destination);
                return conversion.IsIdentity ||
                       conversion.IsImplicit;
            }
        }

        private bool TryFindArgumentType(ITypeParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? type)
        {
            var i = this.Parameters.IndexOf(parameter);
            if (i >= 0)
            {
                return Type.TryGet(this.Arguments[i], semanticModel, cancellationToken, out type, out _);
            }

            type = null;
            return false;
        }
    }
}
