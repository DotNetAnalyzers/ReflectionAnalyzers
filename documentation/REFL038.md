# REFL038
## Prefer RuntimeHelpers.RunClassConstructor.

| Topic    | Value
| :--      | :--
| Id       | REFL038
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [InvokeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs)

## Description

The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().

## Motivation

The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL038 // Prefer RuntimeHelpers.RunClassConstructor.
Code violating the rule here
#pragma warning restore REFL038 // Prefer RuntimeHelpers.RunClassConstructor.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL038 // Prefer RuntimeHelpers.RunClassConstructor.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL038:Prefer RuntimeHelpers.RunClassConstructor.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->