# REFL007
## The binding flags are not in the expected order.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL007</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Hidden</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/BindingFlagsAnalyzer.cs">BindingFlagsAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

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