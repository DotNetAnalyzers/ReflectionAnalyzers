# REFL004
## More than one member is matching the criteria.

| Topic    | Value
| :--      | :--
| Id       | REFL004
| Severity | Warning
| Enabled  | True
| Category | ReflectionAnalyzers.SystemReflection
| Code     | [GetXAnalyzer]([GetXAnalyzer](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/GetXAnalyzer.cs))

## Description

More than one member is matching the criteria.

## Motivation

```cs
public class Foo
{
    public static int Bar(int i) => i;

    public double Bar(double d) => d;

    private byte Bar(byte b) => b;
}
```

If we do `typeof(Foo).GetMethod(nameof(Foo.Bar))` it will throw an `AmbiguousMatchException` at runtime.

There are a couple of ways we can disambiguate:
- `typeof(Foo).GetMethod(nameof(Foo.Bar), new[]{typeof(int));` for matching on argument type.
- `typeof(Foo).GetMethod(nameof(Foo.Bar), BindingFlags.Public | BindingFlags.Instance)` // for filtering on public instance methods.
- `typeof(Foo).GetMethod(nameof(Foo.Bar), BindingFlags.Public | BindingFlags.Static)` // for filtering on public static methods.
- `typeof(Foo).GetMethod(nameof(Foo.Bar), BindingFlags.NonPublic)` // for filtering on private methods.
- `typeof(Foo).GetMethod(nameof(Foo.Bar), BindingFlags.NonPublic)` // for filtering on private methods.

## How to fix violations
Add a combination of `BindingFlags` and or `Type[]` that points to one and only one member.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL004 // More than one member is matching the criteria.
Code violating the rule here
#pragma warning restore REFL004 // More than one member is matching the criteria.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL004 // More than one member is matching the criteria.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection", 
    "REFL004:More than one member is matching the criteria.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
