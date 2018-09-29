namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal struct Flags
    {
        internal readonly ArgumentSyntax Argument;

        /// <summary>
        /// The flags explicitly provided in the argument.
        /// </summary>
        internal readonly BindingFlags Explicit;

        /// <summary>
        /// The default flags for the call if no flag argument was passed.
        /// </summary>
        internal readonly BindingFlags Default;

        public Flags(ArgumentSyntax argument, BindingFlags @explicit, BindingFlags @default)
        {
            this.Argument = argument;
            this.Explicit = @explicit;
            this.Default = @default;
        }

        /// <summary>
        /// The flags used when filtering. <see cref="Explicit"/> if an argument was passed or <see cref="Default"/> if not.
        /// </summary>
        internal BindingFlags Effective => this.Argument != null ? this.Explicit : this.Default;

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

            flags = default(Flags);
            return false;
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
