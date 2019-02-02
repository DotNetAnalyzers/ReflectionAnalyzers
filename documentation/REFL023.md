# REFL023
## The type does not implement the interface.

| Topic    | Value
| :--      | :--
| Id       | REFL023
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetInterfaceAnalyzer]([GetInterfaceAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetInterfaceAnalyzer.cs))

## Description

The type does not implement the interface.

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
#pragma warning disable REFL023 // The type does not implement the interface.
Code violating the rule here
#pragma warning restore REFL023 // The type does not implement the interface.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL023 // The type does not implement the interface.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL023:The type does not implement the interface.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->