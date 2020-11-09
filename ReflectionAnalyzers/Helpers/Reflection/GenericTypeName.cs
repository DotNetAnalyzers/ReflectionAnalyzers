namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;

    internal readonly struct GenericTypeName
    {
        internal readonly string MetadataName;
        internal readonly ImmutableArray<GenericTypeArgument> TypeArguments;

        internal GenericTypeName(string metadataName, ImmutableArray<GenericTypeArgument> typeArguments)
        {
            this.MetadataName = metadataName;
            this.TypeArguments = typeArguments;
        }

        internal static GenericTypeName? TryParse(string text)
        {
            if (text.IndexOf('[') is var index &&
                index > 0)
            {
                var metadataName = text.Substring(0, index);
                if (TryParseArity(metadataName, out var arity) &&
                    GenericTypeArgument.TryParseBracketedList(text, index, arity, out var typeArguments) &&
                    arity == typeArguments.Length)
                {
                    return new GenericTypeName(metadataName, typeArguments);
                }
            }

            return null;
        }

        private static bool TryParseArity(string metadataName, out int result)
        {
            result = -1;
            return metadataName.IndexOf('`') is var i &&
                   i > 0 &&
                   i < metadataName.Length - 1 &&
                   metadataName.Substring(i + 1) is { } substring &&
                   !substring.EndsWith(" ", StringComparison.OrdinalIgnoreCase) &&
                   int.TryParse(metadataName.Substring(i + 1), out result);
        }
    }
}
