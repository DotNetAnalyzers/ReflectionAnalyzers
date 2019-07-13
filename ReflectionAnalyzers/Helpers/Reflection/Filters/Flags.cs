namespace ReflectionAnalyzers
{
    using System.Diagnostics;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DebuggerDisplay("{this.Effective}")]
    internal struct Flags
    {
        internal static readonly Flags MatchAll = new Flags(null, BindingFlags.Default, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        internal readonly ArgumentSyntax Argument;

        /// <summary>
        /// The flags explicitly provided in the argument.
        /// </summary>
        internal readonly BindingFlags Explicit;

        /// <summary>
        /// The default flags for the call if no flag argument was passed.
        /// </summary>
        internal readonly BindingFlags Default;

        internal Flags(ArgumentSyntax argument, BindingFlags @explicit, BindingFlags @default)
        {
            this.Argument = argument;
            this.Explicit = @explicit;
            this.Default = @default;
        }

        /// <summary>
        /// The flags used when filtering. <see cref="Explicit"/> if an argument was passed or <see cref="Default"/> if not.
        /// </summary>
        internal BindingFlags Effective => this.Argument != null ? this.Explicit : this.Default;

        internal bool AreInSufficient
        {
            get
            {
                if (this.Argument == null)
                {
                    return false;
                }

                return !this.Explicit.HasEither(BindingFlags.Public, BindingFlags.NonPublic) ||
                       !this.Explicit.HasEither(BindingFlags.Static, BindingFlags.Instance);
            }
        }

        internal static bool TryCreate(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out Flags flags)
        {
            if (TryGetBindingFlags(invocation, getX, context, out var argument, out var explicitFlags))
            {
                _ = TryGetDefaultBindingFlags(getX.MetadataName, out var defaultFlags);
                flags = new Flags(argument, explicitFlags, defaultFlags);
                return true;
            }
            else if (TryGetDefaultBindingFlags(getX.MetadataName, out var defaultFlags))
            {
                flags = new Flags(argument, (BindingFlags)(-1), defaultFlags);
                return true;
            }

            flags = default;
            return false;
        }

        internal static bool TryGetExpectedBindingFlags(ITypeSymbol reflectedType, ISymbol member, out BindingFlags flags)
        {
            flags = 0;
            if (member is null ||
                reflectedType is null)
            {
                return false;
            }

            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            if (!(member is ITypeSymbol))
            {
                if (member.IsStatic)
                {
                    flags |= BindingFlags.Static;
                }
                else
                {
                    flags |= BindingFlags.Instance;
                }

                if (!(member is IMethodSymbol method &&
                      method.MethodKind == MethodKind.Constructor))
                {
                    if (Equals(member.ContainingType, reflectedType))
                    {
                        flags |= BindingFlags.DeclaredOnly;
                    }
                    else if (member.IsStatic)
                    {
                        flags |= BindingFlags.FlattenHierarchy;
                    }
                }
            }

            return true;
        }

        private static bool TryGetDefaultBindingFlags(string getXName, out BindingFlags flags)
        {
            switch (getXName)
            {
                case "GetConstructor":
                    flags = BindingFlags.Public | BindingFlags.Instance;
                    return true;
                case "GetField":
                case "GetFields":
                case "GetEvent":
                case "GetEvents":
                case "GetMethod":
                case "GetMethods":
                case "GetMember":
                case "GetMembers":
                case "GetNestedType": // https://referencesource.microsoft.com/#mscorlib/system/type.cs,751
                case "GetNestedTypes":
                case "GetProperty":
                case "GetProperties":
                    flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
                    return true;
            }

            flags = 0;
            return false;
        }

        private static bool TryGetBindingFlags(InvocationExpressionSyntax invocation, IMethodSymbol getX, SyntaxNodeAnalysisContext context, out ArgumentSyntax argument, out BindingFlags bindingFlags)
        {
            argument = null;
            bindingFlags = 0;
            return getX.TryFindParameter(KnownSymbol.BindingFlags, out var parameter) &&
                   invocation.TryFindArgument(parameter, out argument) &&
                   context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out bindingFlags);
        }
    }
}
