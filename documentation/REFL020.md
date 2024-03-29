# REFL020
## More than one interface is matching the name

| Topic    | Value
| :--      | :--
| Id       | REFL020
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetInterfaceAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetInterfaceAnalyzer.cs)

## Description

More than one interface is matching the name.

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
#pragma warning disable REFL020 // More than one interface is matching the name
Code violating the rule here
#pragma warning restore REFL020 // More than one interface is matching the name
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL020 // More than one interface is matching the name
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL020:More than one interface is matching the name", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->