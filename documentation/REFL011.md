# REFL011
## Duplicate BindingFlag.

| Topic    | Value
| :--      | :--
| Id       | REFL011
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [BindingFlagsAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/BindingFlagsAnalyzer.cs)

## Description

Duplicate BindingFlag.

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
#pragma warning disable REFL011 // Duplicate BindingFlag.
Code violating the rule here
#pragma warning restore REFL011 // Duplicate BindingFlag.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL011 // Duplicate BindingFlag.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL011:Duplicate BindingFlag.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->