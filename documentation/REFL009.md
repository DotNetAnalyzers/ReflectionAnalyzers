# REFL009
## The referenced member is not known to exist.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL009</td>
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

The referenced member is not known to exist.

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
#pragma warning disable REFL009 // The referenced member is not known to exist.
Code violating the rule here
#pragma warning restore REFL009 // The referenced member is not known to exist.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL009 // The referenced member is not known to exist.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL009:The referenced member is not known to exist.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->