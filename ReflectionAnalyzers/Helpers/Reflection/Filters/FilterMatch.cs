namespace ReflectionAnalyzers;

internal enum FilterMatch
{
    Single,
    NoMatch,
    ExplicitImplementation,
    Ambiguous,
    WrongFlags,
    InSufficientFlags,
    WrongTypes,
    WrongMemberType,
    UseContainingType,
    PotentiallyInvisible,
}
