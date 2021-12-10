# REFL013
## The member is of the wrong type

| Topic    | Value
| :--      | :--
| Id       | REFL013
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs)

## Description

The member is of the wrong type.

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
#pragma warning disable REFL013 // The member is of the wrong type
Code violating the rule here
#pragma warning restore REFL013 // The member is of the wrong type
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL013 // The member is of the wrong type
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL013:The member is of the wrong type", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->