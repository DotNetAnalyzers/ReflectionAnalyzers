# REFL033
## Use the same type as the parameter

| Topic    | Value
| :--      | :--
| Id       | REFL033
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs)

## Description

Use the same type as the parameter.

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
#pragma warning disable REFL033 // Use the same type as the parameter
Code violating the rule here
#pragma warning restore REFL033 // Use the same type as the parameter
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL033 // Use the same type as the parameter
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL033:Use the same type as the parameter", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->