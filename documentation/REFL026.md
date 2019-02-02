# REFL026
## No parameterless constructor defined for this object.

| Topic    | Value
| :--      | :--
| Id       | REFL026
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [ActivatorAnalyzer]([ActivatorAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/ActivatorAnalyzer.cs))

## Description

No parameterless constructor defined for this object.

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
#pragma warning disable REFL026 // No parameterless constructor defined for this object.
Code violating the rule here
#pragma warning restore REFL026 // No parameterless constructor defined for this object.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL026 // No parameterless constructor defined for this object.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL026:No parameterless constructor defined for this object.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->