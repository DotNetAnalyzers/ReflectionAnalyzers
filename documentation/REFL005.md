# REFL005
## There is no member matching the name and binding flags.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL005</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Warning</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>true</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>ReflectionAnalyzers.SystemReflection</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetMethodAnalyzer.cs">GetMethodAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

There is no member matching the name and binding flags.

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
#pragma warning disable REFL005 // There is no member matching the name and binding flags.
Code violating the rule here
#pragma warning restore REFL005 // There is no member matching the name and binding flags.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL005 // There is no member matching the name and binding flags.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL005:There is no member matching the name and binding flags.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->