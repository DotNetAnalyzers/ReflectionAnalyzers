# REFL027
## Prefer Type.EmptyTypes

| Topic    | Value
| :--      | :--
| Id       | REFL027
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [ArgumentAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/ArgumentAnalyzer.cs)

## Description

Prefer Type.EmptyTypes.

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
#pragma warning disable REFL027 // Prefer Type.EmptyTypes
Code violating the rule here
#pragma warning restore REFL027 // Prefer Type.EmptyTypes
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL027 // Prefer Type.EmptyTypes
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL027:Prefer Type.EmptyTypes", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->