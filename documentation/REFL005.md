# REFL005
## There is no member matching the filter.

| Topic    | Value
| :--      | :--
| Id       | REFL005
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer]([GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs))

## Description

There is no member matching the filter.

## Motivation

If we have the following type:
```cs
public class Foo
{
    public int Bar(int i) => i;
}
```

And do `typeof(Foo).GetMethod("WRONG_NAME")` it will return `null`.
This analyzer checks that there is a member matching the name, binding flags and types provided.
It checks at every keystroke so it removes the risk of refactoring accidents.

## How to fix violations

Fix the bug.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL005 // There is no member matching the filter.
Code violating the rule here
#pragma warning restore REFL005 // There is no member matching the filter.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL005 // There is no member matching the filter.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL005:There is no member matching the filter.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
