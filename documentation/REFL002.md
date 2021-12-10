# REFL002
## Discard the return value

| Topic    | Value
| :--      | :--
| Id       | REFL002
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [InvokeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs)

## Description

The method returns void. Discard the return value.

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
#pragma warning disable REFL002 // Discard the return value
Code violating the rule here
#pragma warning restore REFL002 // Discard the return value
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL002 // Discard the return value
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL002:Discard the return value", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->