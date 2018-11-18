# REFL046
## The specified default member does not exist.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>REFL046</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/ReflectionAnalyzers/blob/master/ReflectionAnalyzers/NodeAnalzers/DefaultMemberAttributeAnalyzer.cs">DefaultMemberAttributeAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

The specified default member does not exist, or is not a valid target for InvokeMember.

## Motivation

[`DefaultMemberAttribute`](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.defaultmemberattribute)
defines the default member name used in calls to
[`Type.InvokeMember`](https://docs.microsoft.com/en-us/dotnet/api/system.type.invokemember)
when calling code supplies an empty string for the `name` argument.
`Type.InvokeMember` only accepts some values for `name`:

	- Names
		- Constructor (of supplied type only, no base types)
		- Method (instance or static)
		- Property
		- Field
	- Empty string
	- Dispatch Id for IDispatch members

Calling `Type.InvokeMember` with an empty string will throw an exception
when `DefaultMemberAttribute` does not supply a valid value for `name`.

REFL046 attempts to detect this issue early by verifying that the value given
by `DefaultMemberAttribute` matches the name of a valid target. It checks that
one of the following match the string supplied by `DefaultMemberAttribute`:

	- The type name of the decorated type. This indicates a constructor name
	  match.
	- Members in the decorated type or its base classes, of the following
	  categories:
		- Fields
		- Properties
		- Methods (instance and static)

It does not check for events, as `InvokeMember` does not accept event names.
It does not check for matches to names of base classes, as `InvokeMember` does
not allow creation of base class types when calling `InvokeMember` on a
derived type.

It does not presently check for valid dispatch ID specification.

## How to fix violations

Change the name to one of:

 - The name of the decorated type
 - A member of the decorated type or its base classes, of the following
   categories:
		- Fields
		- Properties
		- Methods (instance or static)

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable REFL046 // The specified default member does not exist.
Code violating the rule here
#pragma warning restore REFL046 // The specified default member does not exist.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable REFL046 // The specified default member does not exist.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReflectionAnalyzers.SystemReflection",
    "REFL046:The specified default member does not exist.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
