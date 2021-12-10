namespace ReflectionAnalyzers
{
    using System.Diagnostics;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Argument}")]
    internal struct Name
    {
        internal static Name Ctor = new(null, ".ctor");
        internal readonly ArgumentSyntax? Argument;
        internal readonly string MetadataName;

        internal Name(ArgumentSyntax? argument, string metadataName)
        {
            this.Argument = argument;
            this.MetadataName = metadataName;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SemanticModel semanticModel, CancellationToken cancellationToken, out Name name)
        {
            if (getX.TryFindParameter(KnownSymbol.String, out var parameter) &&
                invocation.TryFindArgument(parameter, out var argument) &&
                semanticModel.TryGetConstantValue(argument.Expression, cancellationToken, out string? metadataName))
            {
                name = new Name(argument, metadataName!);
                return true;
            }

            name = default;
            return false;
        }
    }
}
