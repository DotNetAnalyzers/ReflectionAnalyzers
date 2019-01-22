# REFL045
## These flags are insufficient to match any members.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL045</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs">GetXAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

These flags are insufficient to match any members.

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
#pragma warning disable REFL045 // These flags are insufficient to match any members.
Code violating the rule here
#pragma warning restore REFL045 // These flags are insufficient to match any members.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL045 // These flags are insufficient to match any members.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL045:These flags are insufficient to match any members.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->