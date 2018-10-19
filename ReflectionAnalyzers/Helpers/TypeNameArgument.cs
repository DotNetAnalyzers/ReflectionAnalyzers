namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Value}")]
    internal struct TypeNameArgument
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly string Value;

        internal TypeNameArgument(ArgumentSyntax argument, string value)
        {
            this.Argument = argument;
            this.Value = value;
        }

        internal bool TryGetGeneric(out string metadataName, out int arity, out ImmutableArray<TypeArgument> typeArguments)
        {
            if (this.Value.IndexOf('[') is var index &&
                index > 0)
            {
                metadataName = this.Value.Substring(0, index);
                return TryParseArity(metadataName, out arity) &&
                       TypeArgument.TryParse(this.Value, index, arity, out typeArguments) &&
                       arity == typeArguments.Length;
            }

            metadataName = null;
            arity = 0;
            return false;

            bool TryParseArity(string text, out int result)
            {
                result = -1;
                return text.IndexOf('`') is var i &&
                       i > 0 &&
                       i < text.Length - 1 &&
                       int.TryParse(text.Substring(i + 1), out result);
            }
        }
    }
}
