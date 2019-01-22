# REFL018
## The member is explicitly implemented.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL018</td>
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