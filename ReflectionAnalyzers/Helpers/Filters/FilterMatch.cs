namespace ReflectionAnalyzers
{
    internal enum FilterMatch
    {
        Single,
        NoMatch,
        ExplicitImplementation,
        Ambiguous,
        WrongFlags,
        WrongTypes,
        WrongMemberType,
        UseContainingType,
        PotentiallyInvisible,
    }
}
