# REFL012
## Prefer Attribute.IsDefined()

| Topic    | Value
| :--      | :--
| Id       | REFL012
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetCustomAttributeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetCustomAttributeAnalyzer.cs)

## Description

Prefer Attribute.IsDefined().

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
#pragma warning disable REFL012 // Prefer Attribute.IsDefined()
Code violating the rule here
#pragma warning restore REFL012 // Prefer Attribute.IsDefined()
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL012 // Prefer Attribute.IsDefined()
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL012:Prefer Attribute.IsDefined()", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->