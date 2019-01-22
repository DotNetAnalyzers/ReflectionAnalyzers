# REFL038
## Prefer RuntimeHelpers.RunClassConstructor.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL038</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Warning</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>True</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>ReflectionAnalyzers.SystemReflection</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs">InvokeAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().

## Motivation

The static constructor should only be run once. Prefer RuntimeHelpers.RunClassConstructor().

## How to fix violations

Us ethe code fix.

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