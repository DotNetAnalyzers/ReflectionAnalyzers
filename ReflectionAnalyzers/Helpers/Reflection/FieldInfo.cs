namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct FieldInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IFieldSymbol Field;

        internal FieldInfo(INamedTypeSymbol reflectedType, IFieldSymbol field)
        {
            this.ReflectedType = reflectedType;
            this.Field = field;
        }

        internal static bool TryGet(ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out FieldInfo fieldInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation when GetX.TryMatchGetField(invocation, context, out var member, out _, out _) &&
                                                                member.Symbol is IFieldSymbol field:
                    fieldInfo = new FieldInfo(member.ReflectedType, field);
                    return true;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var local))
            {
                fieldInfo = default;
                return AssignedValue.TryGetSingle(local, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, context, out fieldInfo);
            }

            fieldInfo = default;
            return false;
        }
    }
}
