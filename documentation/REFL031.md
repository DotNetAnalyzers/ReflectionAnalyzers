# REFL031
## Use generic arguments that satisfies the type parameters

| Topic    | Value
| :--      | :--
| Id       | REFL031
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [MakeGenericAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/MakeGenericAnalyzer.cs)

## Description

Use generic arguments that satisfies the type parameters.

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
#pragma warning disable REFL031 // Use generic arguments that satisfies the type parameters
Code violating the rule here
#pragma warning restore REFL031 // Use generic arguments that satisfies the type parameters
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL031 // Use generic arguments that satisfies the type parameters
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL031:Use generic arguments that satisfies the type parameters", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->