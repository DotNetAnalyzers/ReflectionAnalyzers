namespace ReflectionAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Helper for Type.GetField, Type.GetEvent, Type.GetMember, Type.GetMethod...
    /// </summary>
    internal static class GetX
    {
        internal static bool TryGetConstructorInfo(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? constructor)
        {
            if (TryFindInvocation(memberAccess, KnownSymbol.Type.GetConstructor, semanticModel, cancellationToken, out var invocation) &&
                TryMatchGetConstructor(invocation, semanticModel, cancellationToken, out var member, out _, out _) &&
                member is { Match: FilterMatch.Single, Symbol: IMethodSymbol match })
            {
                constructor = match;
                return true;
            }

            constructor = null;
            return false;
        }

        internal static bool TryGetMethodInfo(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            if (TryFindInvocation(memberAccess, KnownSymbol.Type.GetMethod, semanticModel, cancellationToken, out var invocation) &&
                TryMatchGetMethod(invocation, semanticModel, cancellationToken, out var member, out _, out _, out _) &&
                member is { Match: FilterMatch.Single, Symbol: IMethodSymbol match })
            {
                method = match;
                return true;
            }

            method = null;
            return false;
        }

        internal static bool TryGetNestedType(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out INamedTypeSymbol? type)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess &&
                TryFindInvocation(memberAccess, KnownSymbol.Type.GetNestedType, semanticModel, cancellationToken, out var invocation) &&
                TryMatchGetNestedType(invocation, semanticModel, cancellationToken, out var nestedType, out _, out _) &&
                nestedType is { Match: FilterMatch.Single, Symbol: INamedTypeSymbol namedNested })
            {
                type = namedNested;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static bool TryMatchGetConstructor(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Flags flags, out Types types)
        {
            if (invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, semanticModel, cancellationToken, out var getX))
            {
                if (ReflectedMember.TryGetType(invocation, semanticModel, cancellationToken, out var type, out var typeSource) &&
                    IsKnownSignature(invocation, getX) &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types))
                {
                    return ReflectedMember.TryCreate(getX, invocation, type, typeSource, Name.Ctor, flags.Effective, types, semanticModel.Compilation, out member);
                }

                if (Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    flags.AreInSufficient)
                {
                    member = new ReflectedMember(type, typeSource, null, getX, invocation, FilterMatch.InSufficientFlags);
                    _ = Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types);
                    return true;
                }
            }

            member = default;
            flags = default;
            types = default;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent.
        /// </summary>
        internal static bool TryMatchGetEvent(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, semanticModel, cancellationToken, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField.
        /// </summary>
        internal static bool TryMatchGetField(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, semanticModel, cancellationToken, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static bool TryMatchGetMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            if (invocation.TryGetTarget(KnownSymbol.Type.GetMethod, semanticModel, cancellationToken, out var getX))
            {
                if (ReflectedMember.TryGetType(invocation, semanticModel, cancellationToken, out var type, out var typeSource) &&
                    IsKnownSignature(invocation, getX) &&
                    Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name) &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types))
                {
                    return ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, types, semanticModel.Compilation, out member);
                }

                if (Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    flags.AreInSufficient)
                {
                    _ = Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name);
                    _ = Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types);
                    member = new ReflectedMember(type, typeSource, null, getX, invocation, FilterMatch.InSufficientFlags);
                    return true;
                }
            }

            member = default;
            flags = default;
            name = default;
            types = default;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType.
        /// </summary>
        internal static bool TryMatchGetNestedType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, semanticModel, cancellationToken, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty.
        /// </summary>
        internal static bool TryMatchGetProperty(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            if (invocation.TryGetTarget(KnownSymbol.Type.GetProperty, semanticModel, cancellationToken, out var getX))
            {
                if (ReflectedMember.TryGetType(invocation, semanticModel, cancellationToken, out var type, out var typeSource) &&
                    IsKnownSignature(invocation, getX) &&
                    Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name) &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types))
                {
                    return ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, types, semanticModel.Compilation, out member);
                }

                if (Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    flags.AreInSufficient)
                {
                    _ = Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name);
                    _ = Types.TryCreate(invocation, getX, semanticModel, cancellationToken, out types);
                    member = new ReflectedMember(type, typeSource, null, getX, invocation, FilterMatch.InSufficientFlags);
                    return true;
                }
            }

            member = default;
            flags = default;
            name = default;
            types = default;
            return false;
        }

        private static bool TryFindInvocation(MemberAccessExpressionSyntax memberAccess, QualifiedMethod expected, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? invocation)
        {
            switch (memberAccess.Expression)
            {
                case InvocationExpressionSyntax candidate
                    when candidate.TryGetTarget(expected, semanticModel, cancellationToken, out _):
                    invocation = candidate;
                    return true;
                case IdentifierNameSyntax identifierName
                    when semanticModel.TryGetSymbol(identifierName, cancellationToken, out ILocalSymbol? local) &&
                         AssignedValue.FindSingle(local, semanticModel, cancellationToken) is InvocationExpressionSyntax candidate &&
                         candidate.TryGetTarget(expected, semanticModel, cancellationToken, out _):
                    invocation = candidate;
                    return true;
            }

            invocation = null;
            return false;
        }

        /// <summary>
        /// Defensive check to only handle known cases. Don't know how the binder works.
        /// </summary>
        private static bool IsKnownSignature(InvocationExpressionSyntax invocation, IMethodSymbol getX)
        {
            foreach (var parameter in getX.Parameters)
            {
                if (!IsKnownArgument(parameter))
                {
                    return false;
                }
            }

            return true;
            bool IsKnownArgument(IParameterSymbol parameter)
            {
                if (parameter.Type == KnownSymbol.String ||
                    parameter.Type == KnownSymbol.BindingFlags ||
                    parameter.Name == "types")
                {
                    return true;
                }

                if (parameter.Type == KnownSymbol.Binder &&
                    invocation.TryFindArgument(parameter, out var binderArg))
                {
                    return binderArg.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true ||
                           (binderArg.Expression is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Name.Identifier.ValueText == "DefaultBinder");
                }

                return invocation.TryFindArgument(parameter, out var argument) &&
                       argument.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true;
            }
        }

        /// <summary>
        /// Handles GetField, GetEvent, GetMember, GetMethod...
        /// </summary>
        private static bool TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SemanticModel semanticModel, CancellationToken cancellationToken, out ReflectedMember member, out Name name, out Flags flags)
        {
            if (invocation.TryGetTarget(getXMethod, semanticModel, cancellationToken, out var getX))
            {
                if (ReflectedMember.TryGetType(invocation, semanticModel, cancellationToken, out var type, out var typeSource) &&
                    Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name) &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, Types.Any, semanticModel.Compilation, out member))
                {
                    return true;
                }

                if (getXMethod.Name != "GetNestedType" &&
                    Flags.TryCreate(invocation, getX, semanticModel, cancellationToken, out flags) &&
                    flags.AreInSufficient)
                {
                    _ = Name.TryCreate(invocation, getX, semanticModel, cancellationToken, out name);
                    member = new ReflectedMember(type, typeSource, null, getX, invocation, FilterMatch.InSufficientFlags);
                    return true;
                }
            }

            member = default;
            name = default;
            flags = default;
            return false;
        }
    }
}
