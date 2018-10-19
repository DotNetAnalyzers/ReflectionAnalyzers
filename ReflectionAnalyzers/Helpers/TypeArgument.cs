namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("{this.MetadataName}")]
    internal struct TypeArgument
    {
        private static readonly Regex SimpleTypeNameRegex = new Regex(@"\G,? *(?<typeName>[^ ,\]\[]+ *)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static readonly Regex BracketedTypeNameRegex = new Regex(@"\G,? *\[ *(?<typeName>[^ ,\]]+ *)(?:, [^][]+)*\] *", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal readonly string MetadataName;
        internal readonly IReadOnlyList<TypeArgument> TypeArguments;

        internal TypeArgument(string metadataName, IReadOnlyList<TypeArgument> typeArguments)
        {
            this.MetadataName = metadataName;
            this.TypeArguments = typeArguments;
        }

        internal static bool TryParse(string text, int pos, int arity, out ImmutableArray<TypeArgument> result)
        {
            if (text[pos] == '[' &&
                text[text.Length - 1] == ']')
            {
                pos++;
                var builder = ImmutableArray.CreateBuilder<TypeArgument>(arity);
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

        private static bool TryParse(string text, ref int pos, out TypeArgument typeArgument)
        {
            var match = SimpleTypeNameRegex.Match(text, pos);
            if (match.Success)
            {
                pos = pos + match.Length;
                typeArgument = new TypeArgument(match.Groups["typeName"].Value, null);
                return true;
            }

            match = BracketedTypeNameRegex.Match(text, pos);
            if (match.Success)
            {
                pos = pos + match.Length;
                typeArgument = new TypeArgument(match.Groups["typeName"].Value, null);
                return true;
            }

            pos = int.MaxValue;
            typeArgument = default(TypeArgument);
            return false;
        }

        private static void SkipWhitespace(string text, ref int pos)
        {
            while (pos < text.Length &&
                   text[pos] == ' ')
            {
                pos++;
            }
        }
    }
}
