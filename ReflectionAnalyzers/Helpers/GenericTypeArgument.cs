namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("{this.MetadataName}")]
    internal struct GenericTypeArgument
    {
        private static readonly Regex SimpleTypeNameRegex = new Regex(@"\G,? *(?<typeName>[^ ,\]\[]+ *)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static readonly Regex GenericTypeNameRegex = new Regex(@"\G,? *(?<typeName>[^ `,\]\[]+`(?<arity>\d+))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal readonly string MetadataName;
        internal readonly IReadOnlyList<GenericTypeArgument> TypeArguments;

        internal GenericTypeArgument(string metadataName, IReadOnlyList<GenericTypeArgument> typeArguments)
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

            var match = GenericTypeNameRegex.Match(text, pos);
            if (match.Success)
            {
                if (int.TryParse(match.Groups["arity"].Value, out var arity) &&
                    match.Groups["typeName"].Value is string typeName &&
                    TryFindBracketedList(text, pos, out var argsString) &&
                    TryParseBracketedList(argsString, 0, arity, out var args))
                {
                    pos += typeName.Length + argsString.Length;
                    genericTypeArgument = new GenericTypeArgument(typeName, args);
                    return true;
                }

                genericTypeArgument = default(GenericTypeArgument);
                pos = int.MaxValue;
                return false;
            }

            match = SimpleTypeNameRegex.Match(text, pos);
            if (match.Success)
            {
                pos += match.Length;
                genericTypeArgument = new GenericTypeArgument(match.Groups["typeName"].Value, null);
                return true;
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

            pos = int.MaxValue;
            genericTypeArgument = default(GenericTypeArgument);
            return false;
        }

        private static bool TryFindBracketedList(string text, int start, out string result)
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
