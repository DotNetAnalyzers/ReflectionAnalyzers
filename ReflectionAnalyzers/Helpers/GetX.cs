namespace ReflectionAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Helper for Type.GetField, Type.GetEvent, Type.GetMember, Type.GetMethod...
    /// </summary>
    internal static class GetX
    {
        internal static bool TryGetTargetType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                switch (memberAccess.Expression)
                {
                    case TypeOfExpressionSyntax typeOf:
                        return semanticModel.TryGetType(typeOf.Type, cancellationToken, out result);
                    case InvocationExpressionSyntax getType when getType.TryGetMethodName(out var name) &&
                                                                 name == "GetType" &&
                                                                 getType.ArgumentList is ArgumentListSyntax args &&
                                                                 args.Arguments.Count == 0 &&
                                                                 getType.Expression is MemberAccessExpressionSyntax typeAccess:
                        return semanticModel.TryGetType(typeAccess.Expression, cancellationToken, out result);
                }
            }

            result = null;
            return false;
        }
    }
}
