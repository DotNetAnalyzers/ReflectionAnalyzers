namespace ReflectionAnalyzers
{
    internal enum GetXResult
    {
        Unknown,
        Single,
        NoMatch,
        Ambiguous,
        WrongFlags,
        WrongMemberType,
        UseContainingType,
    }
}