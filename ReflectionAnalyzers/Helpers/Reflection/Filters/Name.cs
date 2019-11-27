namespace ReflectionAnalyzers
{
    using System.Diagnostics;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DebuggerDisplay("{this.Argument}")]
    internal struct Name
    {
        internal static Name Ctor = new Name(null, ".ctor");
        internal readonly ArgumentSyntax Argument;
        internal readonly string MetadataName;

        internal Name(ArgumentSyntax argument, string metadataName)
        {
            this.Argument = argument;
            this.MetadataName = metadataName;
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out Name name)
        {
            if (getX.TryFindParameter(KnownSymbol.String, out var parameter) &&
                invocation.TryFindArgument(parameter, out var argument) &&
                context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string? metadataName))
            {
                name = new Name(argument, metadataName!);
                return true;
            }

            name = default;
            return false;
        }
    }
}
