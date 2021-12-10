# REFL028
## Cast return value to correct type

| Topic    | Value
| :--      | :--
| Id       | REFL028
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [ActivatorAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/ActivatorAnalyzer.cs)
|          | [CreateDelegateAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/CreateDelegateAnalyzer.cs)
|          | [InvokeAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs)

## Description

Cast return value to correct type.

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
#pragma warning disable REFL028 // Cast return value to correct type
Code violating the rule here
#pragma warning restore REFL028 // Cast return value to correct type
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL028 // Cast return value to correct type
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL028:Cast return value to correct type", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->