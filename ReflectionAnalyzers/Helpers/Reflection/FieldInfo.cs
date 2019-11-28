namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct FieldInfo
    {
        internal readonly INamedTypeSymbol ReflectedType;
        internal readonly IFieldSymbol Field;

        internal FieldInfo(INamedTypeSymbol reflectedType, IFieldSymbol field)
        {
            this.ReflectedType = reflectedType;
            this.Field = field;
        }

        internal static bool TryGet(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out FieldInfo fieldInfo)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation when GetX.TryMatchGetField(invocation, semanticModel, cancellationToken, out var member, out _, out _) &&
                                                                member is { ReflectedType: { } reflectedType, Symbol: IFieldSymbol field }:
                    fieldInfo = new FieldInfo(reflectedType, field);
                    return true;
            }

            if (expression.IsEither(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) &&
                semanticModel.TryGetSymbol(expression, cancellationToken, out var local))
            {
                fieldInfo = default;
                return AssignedValue.TryGetSingle(local, semanticModel, cancellationToken, out var assignedValue) &&
                       TryGet(assignedValue, semanticModel, cancellationToken, out fieldInfo);
            }

            fieldInfo = default;
            return false;
        }
    }
}
