# REFL030
## Use correct obj parameter

| Topic    | Value
| :--      | :--
| Id       | REFL030
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [InvokeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs)

## Description

Use correct obj parameter.

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
#pragma warning disable REFL030 // Use correct obj parameter
Code violating the rule here
#pragma warning restore REFL030 // Use correct obj parameter
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL030 // Use correct obj parameter
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL030:Use correct obj parameter", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->