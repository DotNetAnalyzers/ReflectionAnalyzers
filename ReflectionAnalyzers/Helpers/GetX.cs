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
        /// <summary>
        /// Returns Foo for the invocation typeof(Foo).GetProperty(Bar)
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="semanticModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static bool TryGetDeclaringType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
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
                                                                 args.Arguments.Count == 0:
                        switch (getType.Expression)
                        {
                            case MemberAccessExpressionSyntax typeAccess:
                                return semanticModel.TryGetType(typeAccess.Expression, cancellationToken, out result);
                            case IdentifierNameSyntax _ when invocation.TryFirstAncestor(out TypeDeclarationSyntax containingType):
                                return semanticModel.TryGetSymbol(containingType, cancellationToken, out result);
                        }

                        break;
                }
            }

            result = null;
            return false;
        }
    }
}
