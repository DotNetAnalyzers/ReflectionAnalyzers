namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    }
}
