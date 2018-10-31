namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct PropertyInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IPropertySymbol Property;

        public PropertyInfo(INamedTypeSymbol reflectedType, IPropertySymbol property)
        {
            this.ReflectedType = reflectedType;
            this.Property = property;
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out PropertyInfo propertyInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation when GetX.TryMatchGetProperty(invocation, context, out var member, out _, out _, out _) &&
                                                                member.Symbol is IPropertySymbol property:
                    propertyInfo = new PropertyInfo(member.ReflectedType, property);
                    return true;
                default:
                    propertyInfo = default(PropertyInfo);
                    return false;
            }
        }
    }
}
