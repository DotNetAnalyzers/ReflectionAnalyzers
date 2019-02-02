# REFL024
## Prefer null over empty array.

| Topic    | Value
| :--      | :--
| Id       | REFL024
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [InvokeAnalyzer]([InvokeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs))

## Description

Prefer null over empty array.

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
#pragma warning disable REFL024 // Prefer null over empty array.
Code violating the rule here
#pragma warning restore REFL024 // Prefer null over empty array.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL024 // Prefer null over empty array.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL024:Prefer null over empty array.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->