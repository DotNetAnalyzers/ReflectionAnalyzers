namespace ReflectionAnalyzers
{
    using System.Collections.Generic;

    internal struct GenericArgument
    {
        internal readonly string MetadataName;

        internal readonly IReadOnlyList<GenericArgument> TypeArguments;

        internal GenericArgument(string metadataName, IReadOnlyList<GenericArgument> typeArguments)
        {
            this.MetadataName = metadataName;
            this.TypeArguments = typeArguments;
        }
    }
}
