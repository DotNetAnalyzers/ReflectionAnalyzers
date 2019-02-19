# REFL040
## Prefer type.IsInstanceOfType(...).

| Topic    | Value
| :--      | :--
| Id       | REFL040
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [IsAssignableFromAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/IsAssignableFromAnalyzer.cs)

## Description

Prefer type.IsInstanceOfType(...).

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
#pragma warning disable REFL040 // Prefer type.IsInstanceOfType(...).
Code violating the rule here
#pragma warning restore REFL040 // Prefer type.IsInstanceOfType(...).
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL040 // Prefer type.IsInstanceOfType(...).
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL040:Prefer type.IsInstanceOfType(...).", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->