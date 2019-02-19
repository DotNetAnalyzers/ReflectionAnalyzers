# REFL029
## Specify types in case an overload is added in the future.

| Topic    | Value
| :--      | :--
| Id       | REFL029
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs)

## Description

Specify types in case an overload is added in the future.

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
#pragma warning disable REFL029 // Specify types in case an overload is added in the future.
Code violating the rule here
#pragma warning restore REFL029 // Specify types in case an overload is added in the future.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL029 // Specify types in case an overload is added in the future.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL029:Specify types in case an overload is added in the future.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->