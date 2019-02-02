# REFL034
## Don't call MakeGeneric when not a generic definition.

| Topic    | Value
| :--      | :--
| Id       | REFL034
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [MakeGenericAnalyzer]([MakeGenericAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/MakeGenericAnalyzer.cs))

## Description

Don't call MakeGeneric when not a generic definition.

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
#pragma warning disable REFL034 // Don't call MakeGeneric when not a generic definition.
Code violating the rule here
#pragma warning restore REFL034 // Don't call MakeGeneric when not a generic definition.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL034 // Don't call MakeGeneric when not a generic definition.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL034:Don't call MakeGeneric when not a generic definition.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->