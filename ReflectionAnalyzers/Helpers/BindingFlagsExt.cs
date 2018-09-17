namespace ReflectionAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class BindingFlagsExt
    {
        internal static bool HasFlagFast(this BindingFlags flags, BindingFlags flag)
        {
            return (flags & flag) != 0;
        }

        internal static string ToDisplayString(this BindingFlags flags)
        {
            var stringBuilder = StringBuilderPool.Borrow();
            AppendIfHasFlag(BindingFlags.Public);
            AppendIfHasFlag(BindingFlags.NonPublic);
            AppendIfHasFlag(BindingFlags.Static);
            AppendIfHasFlag(BindingFlags.Instance);
            AppendIfHasFlag(BindingFlags.DeclaredOnly);
            AppendIfHasFlag(BindingFlags.FlattenHierarchy);
            AppendIfHasFlag(BindingFlags.IgnoreCase);

            return stringBuilder.Return();
            void AppendIfHasFlag(BindingFlags flag)
            {
                if (flags.HasFlagFast(flag))
                {
                    if (stringBuilder.Length != 0)
                    {
                        _ = stringBuilder.Append(" | ");
                    }

                    _ = stringBuilder.Append("BindingFlags.").Append(flag.Name());
                }
            }
        }

        internal static string Name(this BindingFlags flag)
        {
            switch (flag)
            {
                case BindingFlags.DeclaredOnly:
                    return "DeclaredOnly";
                case BindingFlags.FlattenHierarchy:
                    return "FlattenHierarchy";
                case BindingFlags.IgnoreCase:
                    return "IgnoreCase";
                case BindingFlags.Instance:
                    return "Instance";
                case BindingFlags.NonPublic:
                    return "NonPublic";
                case BindingFlags.Public:
                    return "Public";
                case BindingFlags.Static:
                    return "Static";
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }
        }
    }
}
