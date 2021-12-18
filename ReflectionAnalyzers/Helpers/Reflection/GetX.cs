namespace ReflectionAnalyzers
{
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
