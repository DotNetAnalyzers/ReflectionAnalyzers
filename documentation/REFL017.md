# REFL017
## Don't use name of wrong member.

| Topic    | Value
| :--      | :--
| Id       | REFL017
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer]([GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs))

## Description

Don't use name of wrong member.

## Motivation

```cs
public class Foo
{
    public Foo()
    {
        var member = typeof(AggregateException).GetProperty(
            ↓nameof(this.InnerExceptionCount), 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    public int InnerExceptionCount => 0;
}
```

In the above example we are getting the `private` property [`AggregateException.InnerExceptionCount`](https://referencesource.microsoft.com/#mscorlib/system/AggregateException.cs,466) but using `nameof(this.InnerExceptionCount)` to get the name. This rule checks that the same member is used. 

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL017 // Don't use name of wrong member.
Code violating the rule here
#pragma warning restore REFL017 // Don't use name of wrong member.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL017 // Don't use name of wrong member.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL017:Don't use name of wrong member.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
