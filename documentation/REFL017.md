# REFL017
## Don't use nameof.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL017</td>
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

Don't use nameof.

## Motivation

```cs
public class Foo
{
    public Foo()
    {
        var member = typeof(AggregateException).GetProperty(
            ↓nameof(this.InnerExceptionCount), 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    public int InnerExceptionCount => 0;
}
```

In the above example we are getting the `private` property [`AggregateException.InnerExceptionCount`](https://referencesource.microsoft.com/#mscorlib/system/AggregateException.cs,466) but using `nameof(this.InnerExceptionCount) to get the name. 

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL017 // Don't use nameof.
Code violating the rule here
#pragma warning restore REFL017 // Don't use nameof.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL017 // Don't use nameof.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL017:Don't use nameof.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
