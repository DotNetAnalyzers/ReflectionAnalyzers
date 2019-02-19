# REFL018
## The member is explicitly implemented.

| Topic    | Value
| :--      | :--
| Id       | REFL018
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs)

## Description

The member is explicitly implemented.

## Motivation

`typeof(string).GetMethod(nameof(IConvertible.ToBoolean))` returns null as `ToBoolean` is explicitly implemented.

## How to fix violations

Change to `typeof(IConvertible).GetMethod(nameof(IConvertible.ToBoolean))` or use the interface map to find the member.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL018 // The member is explicitly implemented.
Code violating the rule here
#pragma warning restore REFL018 // The member is explicitly implemented.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL018 // The member is explicitly implemented.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL018:The member is explicitly implemented.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->