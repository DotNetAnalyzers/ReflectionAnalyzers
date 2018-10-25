# REFL036
## Pass 'throwOnError: true' or check if null.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL036</td>
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

Pass 'throwOnError: true' or check if null.

## Motivation

```cs
var mi = Type.GetType("Foo").GetMethod("Bar");
```

In the above example `Type.GetType` may return null.

## How to fix violations

Use the code fix to change it to:

```cs
var mi = Type.GetType("Foo", throwOnError: true).GetMethod("Bar");
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL036 // Pass 'throwOnError: true' or check if null.
Code violating the rule here
#pragma warning restore REFL036 // Pass 'throwOnError: true' or check if null.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL036 // Pass 'throwOnError: true' or check if null.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL036:Pass 'throwOnError: true' or check if null.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->