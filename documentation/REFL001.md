# REFL001
## Cast return value to the correct type.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL001</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/ActivatorAnalyzer.cs">ActivatorAnalyzer</a></td>
  </tr>
  <tr>
    <td></td>
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/CreateDelegateAnalyzer.cs">CreateDelegateAnalyzer</a></td>
  </tr>
  <tr>
    <td></td>
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/InvokeAnalyzer.cs">InvokeAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Cast return value to the correct type.

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
#pragma warning disable REFL001 // Cast return value to the correct type.
Code violating the rule here
#pragma warning restore REFL001 // Cast return value to the correct type.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL001 // Cast return value to the correct type.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL001:Cast return value to the correct type.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->