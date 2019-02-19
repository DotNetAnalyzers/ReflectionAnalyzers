# REFL007
## The binding flags are not in the expected order.

| Topic    | Value
| :--      | :--
| Id       | REFL007
| Severity | Hidden
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [BindingFlagsAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/BindingFlagsAnalyzer.cs)

## Description

The binding flags are not in the expected order.

## Motivation

Not a very useful analyzer fix. Consistency is the only motivation.

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL007 // The binding flags are not in the expected order.
Code violating the rule here
#pragma warning restore REFL007 // The binding flags are not in the expected order.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL007 // The binding flags are not in the expected order.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL007:The binding flags are not in the expected order.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->