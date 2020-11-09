namespace ReflectionAnalyzers
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Value}")]
    internal readonly struct TypeNameArgument
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly string Value;

        internal TypeNameArgument(ArgumentSyntax argument, string value)
        {
            this.Argument = argument;
            this.Value = value;
        }

        internal GenericTypeName? TryGetGeneric()
        {
            return GenericTypeName.TryParse(this.Value);
        }
    }
}
