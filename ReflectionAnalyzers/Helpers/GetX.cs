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
        internal static bool TryGetConstructor(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol constructor)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                var result = TryMatchGetConstructor(parentInvocation, context, out var member, out _, out _);
                if (result == FilterMatch.Single &&
                    member.Symbol is IMethodSymbol match)
                {
                    constructor = match;
                    return true;
                }
            }

            constructor = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static FilterMatch? TryMatchGetConstructor(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Flags flags, out Types types)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetConstructor, context.SemanticModel, context.CancellationToken, out var getX) &&
                ReflectedMember.TryGetType(invocation, context, out var type, out _) &&
                IsKnownSignature(invocation, getX) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                Types.TryCreate(invocation, getX, context, out types))
            {
                var result = ReflectedMember.TryGetMember(getX, type, new Name(null, ".ctor"), flags.Effective, types, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            types = default(Types);
            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetEvent.
        /// </summary>
        internal static FilterMatch? TryMatchGetEvent(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetEvent, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetField.
        /// </summary>
        internal static FilterMatch? TryMatchGetField(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetField, context, out member, out name, out flags);
        }

        internal static bool TryGetMethod(MemberAccessExpressionSyntax memberAccess, SyntaxNodeAnalysisContext context, out IMethodSymbol method)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                var result = TryMatchGetMethod(parentInvocation, context, out var member, out _, out _, out _);
                if (result == FilterMatch.Single &&
                    member.Symbol is IMethodSymbol match)
                {
                    method = match;
                    return true;
                }
            }

            method = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetMethod.
        /// </summary>
        internal static FilterMatch? TryMatchGetMethod(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags, out Types types)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
                ReflectedMember.TryGetType(invocation, context, out var type, out _) &&
                IsKnownSignature(invocation, getX) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags) &&
                Types.TryCreate(invocation, getX, context, out types))
            {
                if (type == KnownSymbol.Delegate &&
                    name.MetadataName == "Invoke")
                {
                    member = new ReflectedMember(type, null);
                    return FilterMatch.Single;
                }

                var result = ReflectedMember.TryGetMember(getX, type, name, flags.Effective, types, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            member = default(ReflectedMember);
            flags = default(Flags);
            name = default(Name);
            types = default(Types);
            return null;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetNestedType.
        /// </summary>
        internal static FilterMatch? TryMatchGetNestedType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            return TryMatchGetX(invocation, KnownSymbol.Type.GetNestedType, context, out member, out name, out flags);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to Type.GetProperty.
        /// </summary>
        internal static FilterMatch? TryMatchGetProperty(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
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
                memberAccess.Expression is InvocationExpressionSyntax parentInvocation &&
                GetX.TryMatchGetNestedType(parentInvocation, context, out var nestedType, out _, out _) == FilterMatch.Single &&
                nestedType.Symbol is INamedTypeSymbol namedNested)
            {
                type = namedNested;
                return true;
            }

            type = null;
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
        private static FilterMatch? TryMatchGetX(InvocationExpressionSyntax invocation, QualifiedMethod getXMethod, SyntaxNodeAnalysisContext context, out ReflectedMember member, out Name name, out Flags flags)
        {
            if (invocation.ArgumentList != null &&
                invocation.TryGetTarget(getXMethod, context.SemanticModel, context.CancellationToken, out var getX) &&
               ReflectedMember.TryGetType(invocation, context, out var type, out _) &&
                Name.TryCreate(invocation, getX, context, out name) &&
                Flags.TryCreate(invocation, getX, context, out flags))
            {
                var result = ReflectedMember.TryGetMember(getX, type, name, flags.Effective, Types.Any, context, out var symbol);
                member = new ReflectedMember(type, symbol);
                return result;
            }

            name = default(Name);
            member = default(ReflectedMember);
            flags = default(Flags);
            return null;
        }
    }
}
