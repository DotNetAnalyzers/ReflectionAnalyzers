# REFL039
## Prefer typeof(...) over instance.GetType when the type is sealed.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL039</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetTypeAnalyzer.cs">GetTypeAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Prefer typeof(...) over instance.GetType when the type is sealed.

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
#pragma warning disable REFL039 // Prefer typeof(...) over instance.GetType when the type is sealed.
Code violating the rule here
#pragma warning restore REFL039 // Prefer typeof(...) over instance.GetType when the type is sealed.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL039 // Prefer typeof(...) over instance.GetType when the type is sealed.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL039:Prefer typeof(...) over instance.GetType when the type is sealed.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->