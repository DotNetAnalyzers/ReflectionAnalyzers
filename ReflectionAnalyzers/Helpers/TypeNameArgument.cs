namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct TypeNameArgument
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly string Value;

        internal TypeNameArgument(ArgumentSyntax argument, string value)
        {
            this.Argument = argument;
            this.Value = value;
        }

        internal bool TryGetGeneric(out string metadataName, out int arity, out ImmutableArray<GenericArgument> typeArguments)
        {
            if (this.Value.IndexOf('[') is var index &&
                index > 0)
            {
                metadataName = this.Value.Substring(0, index);
                return TryGetArity(metadataName, out arity) &&
                       TryGetArguments(this.Value.Substring(index - 1), arity, out typeArguments) &&
                       arity == typeArguments.Length;
            }

            metadataName = null;
            arity = 0;
            return false;

            bool TryGetArity(string text, out int result)
            {
                var match = Regex.Match(text, "`(?<arity>\\d+)$");
                if (!match.Success)
                {
                    text = null;
                    result = 0;
                    return false;
                }

                result = int.Parse(match.Groups["arity"].Value, CultureInfo.InvariantCulture);
                return true;
            }

            bool TryGetArguments(string text, int n, out ImmutableArray<GenericArgument> result)
            {
                var matches = Regex.Matches(text, @"\[(?<name>\w+)\]");
                if (matches.Count != n)
                {
                    return false;
                }

                var builder = ImmutableArray.CreateBuilder<GenericArgument>(n);
                foreach (Match match in matches)
                {
                    if (!match.Success)
                    {
                        return false;
                    }

                    builder.Add(new GenericArgument(match.Groups["name"].Value, null));
                }

                result = builder.MoveToImmutable();
                return true;
            }
        }
    }
}
