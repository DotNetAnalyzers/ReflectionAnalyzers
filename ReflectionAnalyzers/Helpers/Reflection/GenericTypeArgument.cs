namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [DebuggerDisplay("{this.MetadataName}")]
    internal struct GenericTypeArgument
    {
        internal readonly string MetadataName;
        internal readonly IReadOnlyList<GenericTypeArgument> TypeArguments;

        private GenericTypeArgument(string metadataName, IReadOnlyList<GenericTypeArgument> typeArguments)
        {
            this.MetadataName = metadataName;
            this.TypeArguments = typeArguments;
        }

        internal static bool TryParseBracketedList(string text, int pos, int arity, out ImmutableArray<GenericTypeArgument> result)
        {
            if (text[pos] == '[' &&
                text[text.Length - 1] == ']')
            {
                pos++;
                var builder = ImmutableArray.CreateBuilder<GenericTypeArgument>(arity);
                while (pos < text.Length - 1 &&
                       TryParse(text, ref pos, out var arg))
                {
                    builder.Add(arg);
                }

                if (pos == text.Length - 1 &&
                    arity == builder.Count)
                {
                    result = builder.MoveToImmutable();
                    return true;
                }
            }

            return false;
        }

        private static bool TryParse(string text, ref int pos, out GenericTypeArgument genericTypeArgument)
        {
            while (pos < text.Length &&
                   (text[pos] == ' ' || text[pos] == ','))
            {
                pos++;
            }

            if (text[pos] == '[' &&
                TryFindBracketedList(text, pos, out var bracketed))
            {
                var temp = 1;
                if (TryParse(bracketed, ref temp, out genericTypeArgument))
                {
                    pos += bracketed.Length;
                    while (pos < text.Length &&
                           text[pos] == ' ')
                    {
                        pos++;
                    }

                    return true;
                }
            }
            else if (TryFindMetadataName(text, ref pos, out var metadataName, out var arity))
            {
                if (arity > 0 &&
                    TryFindBracketedList(text, pos, out var argsString) &&
                    TryParseBracketedList(argsString, 0, arity, out var args))
                {
                    genericTypeArgument = new GenericTypeArgument(metadataName, args);
                    pos += argsString.Length;
                    return true;
                }

                genericTypeArgument = new GenericTypeArgument(metadataName, null);
                return true;
            }

            pos = int.MaxValue;
            genericTypeArgument = default;
            return false;
        }

        private static bool TryFindMetadataName(string text, ref int pos, [NotNullWhen(true)] out string? metadataName, out int arity)
        {
            if (text.IndexOfAny(new[] { ',', '[', ']' }, pos) is var index &&
                text.TrySlice(pos, index - 1, out metadataName))
            {
                pos += metadataName.Length;
                if (text[index] == '[')
                {
                    return TryParseArity(metadataName, out arity);
                }

                arity = 0;
                return true;
            }

            metadataName = null;
            arity = 0;
            pos = int.MaxValue;
            return false;
        }

        private static bool TryParseArity(string text, out int arity)
        {
            var start = text.Length - 1;
            while (start > 2 &&
                   char.IsDigit(text[start]))
            {
                start--;
            }

            if (text[start] != '`')
            {
                arity = 0;
                return false;
            }

            arity = int.Parse(text.Substring(start + 1), CultureInfo.InvariantCulture);
            return true;
        }

        private static bool TryFindBracketedList(string text, int start, [NotNullWhen(true)] out string? result)
        {
            // Span opportunity here.
            var level = 0;
            for (var i = start; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '[':
                        if (level == 0)
                        {
                            start = i;
                        }

                        level++;
                        break;
                    case ']':
                        level--;
                        if (level < 0)
                        {
                            result = null;
                            return false;
                        }

                        if (level == 0)
                        {
                            result = text.Slice(start, i);
                            return true;
                        }

                        break;
                }
            }

            result = null;
            return false;
        }
    }
}
