namespace ReflectionAnalyzers
{
    internal enum FilterMatch
    {
        Unknown,
        Single,
        NoMatch,
        ExplicitImplementation,
        Ambiguous,
        WrongFlags,
        WrongTypes,
        WrongMemberType,
        UseContainingType,
    }
}
