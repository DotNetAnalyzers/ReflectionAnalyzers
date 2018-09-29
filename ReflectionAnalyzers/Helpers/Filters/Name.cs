namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct Name
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly string MetadataName;

        public Name(ArgumentSyntax argument, string metadataName)
        {
            this.Argument = argument;
            this.MetadataName = metadataName;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out Name name)
        {
            if (getX.TryFindParameter(KnownSymbol.String, out var parameter) &&
                invocation.TryFindArgument(parameter, out var argument) &&
                context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string metadataName))
            {
                name = new Name(argument, metadataName);
                return true;
            }

            name = default(Name);
            return false;
        }

        /// <summary>
        /// Trims generic part of <see cref="MetadataName"/> as typeSymbol.GetMembers() does not work with it.
        /// IEnumerable`1 becomes IEnumerable.
        /// </summary>
        /// <returns></returns>
        internal string MemberName()
        {
            var index = this.MetadataName.IndexOf('`');
            if (index > 0)
            {
                return this.MetadataName.Substring(0, index);
            }

            return this.MetadataName;
        }
    }
}
