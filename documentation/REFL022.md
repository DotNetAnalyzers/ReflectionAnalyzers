# REFL022
## Use fully qualified name

| Topic    | Value
| :--      | :--
| Id       | REFL022
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetInterfaceAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetInterfaceAnalyzer.cs)

## Description

Use fully qualified name.

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
#pragma warning disable REFL022 // Use fully qualified name
Code violating the rule here
#pragma warning restore REFL022 // Use fully qualified name
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL022 // Use fully qualified name
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL022:Use fully qualified name", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->