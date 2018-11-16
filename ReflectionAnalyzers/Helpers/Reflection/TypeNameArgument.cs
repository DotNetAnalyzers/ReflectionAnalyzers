namespace ReflectionAnalyzers
{
    using System.Diagnostics;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Value.FullName}")]
    internal struct TypeNameArgument
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly string Value;

        internal TypeNameArgument(ArgumentSyntax argument, string value)
        {
            this.Argument = argument;
            this.Value = value;
        }

        internal bool TryGetGeneric(out GenericTypeName genericTypeName)
        {
            return GenericTypeName.TryParse(this.Value, out genericTypeName);
        }
    }
}
