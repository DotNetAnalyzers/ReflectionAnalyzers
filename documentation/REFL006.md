# REFL006
## The binding flags can be more precise.

| Topic    | Value
| :--      | :--
| Id       | REFL006
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer]([GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs))

## Description

The binding flags can be more precise.

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
#pragma warning disable REFL006 // The binding flags can be more precise.
Code violating the rule here
#pragma warning restore REFL006 // The binding flags can be more precise.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL006 // The binding flags can be more precise.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL006:The binding flags can be more precise.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->