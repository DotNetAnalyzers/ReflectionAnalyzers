# REFL032
## The dependency does not exist.

| Topic    | Value
| :--      | :--
| Id       | REFL032
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [DependencyAttributeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/DependencyAttributeAnalyzer.cs)

## Description

The dependency does not exist.

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
#pragma warning disable REFL032 // The dependency does not exist.
Code violating the rule here
#pragma warning restore REFL032 // The dependency does not exist.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL032 // The dependency does not exist.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL032:The dependency does not exist.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->