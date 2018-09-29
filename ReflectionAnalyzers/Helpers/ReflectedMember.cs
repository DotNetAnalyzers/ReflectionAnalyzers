namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal struct ReflectedMember
    {
        /// <summary>
        /// The type that was used to obtain <see cref="Symbol"/>.
        /// </summary>
        internal readonly ITypeSymbol ReflectedType;

        internal readonly ISymbol Symbol;

        public ReflectedMember(ITypeSymbol reflectedType, ISymbol symbol)
        {
            this.ReflectedType = reflectedType;
            this.Symbol = symbol;
        }
    }
}
