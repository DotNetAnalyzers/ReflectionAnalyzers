namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Helper for Type.GetField, Type.GetEvent, Type.GetMember, Type.GetMethod...
    /// </summary>
    internal static class GetX
    {
        internal static bool TryGetConstructorInfo(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol constructor)
        {
            if (TryFindInvocation(memberAccess, KnownSymbol.Type.GetConstructor, context, out var invocation) &&
                TryMatchGetConstructor(invocation, context, out var member, out _, out _) &&
                member.Match == FilterMatch.Single &&
                member.Symbol is IMethodSymbol match)
            {
                constructor = match;
                return true;
            }

            constructor = null;
            return false;
        }

        internal static bool TryGetMethodInfo(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol method)
        {
            if (TryFindInvocation(memberAccess, KnownSymbol.Type.GetMethod, context, out var invocation) &&
                TryMatchGetMethod(invocation, context, out var member, out _, out _, out _) &&
                member.Match == FilterMatch.Single &&
                member.Symbol is IMethodSymbol match)
            {
                method = match;
                return true;
            }

            method = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static bool TryMatchGetConstructor(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Flags flags, out Types types)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, context.SemanticModel, context.CancellationToken, out var getX) &&
                ReflectedMember.TryGetType(invocation, context, out var type, out var typeSource) &&
                IsKnownSignature(invocation, getX) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                Types.TryCreate(invocation, getX, context, out types))
            {
                return ReflectedMember.TryCreate(getX, invocation, type, typeSource, new Name(null, ".ctor"), flags.Effective, types, context, out member);
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            types = default(Types);
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent.
        /// </summary>
        internal static bool TryMatchGetEvent(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField.
        /// </summary>
        internal static bool TryMatchGetField(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static bool TryMatchGetMethod(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                ReflectedMember.TryGetType(invocation, context, out var type, out var typeSource) &&
                IsKnownSignature(invocation, getX) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                Types.TryCreate(invocation, getX, context, out types))
            {
                return ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, types, context, out member);
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            name = default(Name);
            types = default(Types);
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType.
        /// </summary>
        internal static bool TryMatchGetNestedType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty.
        /// </summary>
        internal static bool TryMatchGetProperty(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetProperty, context, out member, out name, out flags);
        }

        internal static bool TryGetType(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out INamedTypeSymbol type)
        {
            if (Type.TryGet(expression, context, out var temp, out _) &&
                temp is INamedTypeSymbol namedType)
            {
                type = namedType;
                return true;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess &&
                TryFindInvocation(memberAccess, KnownSymbol.Type.GetNestedType, context, out var invocation) &&
                TryMatchGetNestedType(invocation, context, out var nestedType, out _, out _) &&
                nestedType.Match == FilterMatch.Single &&
                nestedType.Symbol is INamedTypeSymbol namedNested)
            {
                type = namedNested;
                return true;
            }

            type = null;
            return false;
        }

        private static bool TryFindInvocation(MemberAccessExpressionSyntax memberAccess, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out InvocationExpressionSyntax invocation)
        {
            switch (memberAccess.Expression)
            {
                case InvocationExpressionSyntax candidate when candidate.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out _):
                    invocation = candidate;
                    return true;
                case IdentifierNameSyntax identifierName when context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out ILocalSymbol local) &&
                                                              AssignedValueWalker.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var expression) &&
                                                              expression is InvocationExpressionSyntax candidate &&
                                                              candidate.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out _):
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

                return invocation.TryFindArgument(parameter, out var argument) &&
                       argument.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true;
            }
        }

        /// <summary>
        /// Handles GetField, GetEvent, GetMember, GetMethod...
        /// </summary>
        private static bool TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                ReflectedMember.TryGetType(invocation, context, out var type, out var typeSource) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                ReflectedMember.TryCreate(getX, invocation, type, typeSource, name, flags.Effective, Types.Any, context, out member))
            {
                return true;
            }

            name = default(Name);
            member = default(ReflectedMember);
            flags = default(Flags);
            return false;
        }
    }
}
