# REFL019
## No member matches the types

| Topic    | Value
| :--      | :--
| Id       | REFL019
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs)

## Description

No member matches the types.

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL019 // No member matches the types
Code violating the rule here
#pragma warning restore REFL019 // No member matches the types
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL019 // No member matches the types
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL019:No member matches the types", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->