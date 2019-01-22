# REFL037
## The type does not exist.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL037</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetTypeAnalyzer.cs">GetTypeAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

The type does not exist.

## Motivation

```cs
var type = Type.GetType("Int32");
```

In the above the type name is not qualified and is not found at runtime. Should be:

```cs
var type = Type.GetType("System.Int32");
```

## How to fix violations

Use the code fix to pick from suggested types or edit so that the string points to an existing type.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL037 // The type does not exist.
Code violating the rule here
#pragma warning restore REFL037 // The type does not exist.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL037 // The type does not exist.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL037:The type does not exist.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->