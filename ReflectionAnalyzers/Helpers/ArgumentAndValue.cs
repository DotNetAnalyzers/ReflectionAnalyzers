namespace ReflectionAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct ArgumentAndValue<T>
    {
        internal readonly ArgumentSyntax Argument;

        internal readonly T Value;

        internal ArgumentAndValue(ArgumentSyntax argument, T value)
        {
            this.Argument = argument;
            this.Value = value;
        }
    }
}
