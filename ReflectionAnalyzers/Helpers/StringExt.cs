namespace ReflectionAnalyzers;

using System.Diagnostics.CodeAnalysis;

internal static class StringExt
{
    internal static bool TrySlice(this string text, int start, int end, [NotNullWhen(true)] out string? slice)
    {
        if (end > start)
        {
            slice = text.Substring(start, end - start + 1);
            return true;
        }

        slice = null;
        return false;
    }

    internal static string Slice(this string text, int start, int end) => text.Substring(start, end - start + 1);
}
