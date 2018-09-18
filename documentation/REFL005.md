# REFL005
## There is no member matching the filter.

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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs">GetXAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

There is no member matching the filter.

## Motivation

If we have the following type:
```cs
public class Foo
{
    public int Bar(int i) => i;
}
```

And do `typeof(Foo).GetMethod("WRONG_NAME")` it will return `null`.
This analyzer checks that there is a member matching the name, binding flags and types provided.
It checks at every keystroke so it removes the risk of refactroing accidents.

## How to fix violations

Fix the bug.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL005 // There is no member matching the filter.
Code violating the rule here
#pragma warning restore REFL005 // There is no member matching the filter.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL005 // There is no member matching the filter.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL005:There is no member matching the filter.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->