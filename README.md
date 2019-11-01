
# ReflectionAnalyzers
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/5apvp4qa64q3tyi8/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/reflectionanalyzers/branch/master)
[![Build Status](https://dev.azure.com/DotNetAnalyzers/ReflectionAnalyzers/_apis/build/status/DotNetAnalyzers.ReflectionAnalyzers?branchName=master)](https://dev.azure.com/DotNetAnalyzers/ReflectionAnalyzers/_build/latest?definitionId=5&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/ReflectionAnalyzers.svg)](https://www.nuget.org/packages/ReflectionAnalyzers/)
[![Join the chat at https://gitter.im/DotNetAnalyzers/ReflectionAnalyzers](https://badges.gitter.im/DotNetAnalyzers/ReflectionAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/ReflectionAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Analyzers checking System.Reflection

| Id       | Title
| :--      | :--
| [REFL001](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL001.md)| Cast return value to the correct type.
| [REFL002](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL002.md)| Discard the return value.
| [REFL003](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL003.md)| The member does not exist.
| [REFL004](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL004.md)| More than one member is matching the criteria.
| [REFL005](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL005.md)| There is no member matching the filter.
| [REFL006](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL006.md)| The binding flags can be more precise.
| [REFL007](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL007.md)| The binding flags are not in the expected order.
| [REFL008](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL008.md)| Specify binding flags for better performance and less fragile code.
| [REFL009](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL009.md)| The referenced member is not known to exist.
| [REFL010](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL010.md)| Prefer the generic extension method GetCustomAttribute<T>.
| [REFL011](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL011.md)| Duplicate BindingFlag.
| [REFL012](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL012.md)| Prefer Attribute.IsDefined().
| [REFL013](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL013.md)| The member is of the wrong type.
| [REFL014](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL014.md)| Prefer GetMember().AccessorMethod.
| [REFL015](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL015.md)| Use the containing type.
| [REFL016](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL016.md)| Use nameof.
| [REFL017](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL017.md)| Don't use name of wrong member.
| [REFL018](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL018.md)| The member is explicitly implemented.
| [REFL019](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL019.md)| No member matches the types.
| [REFL020](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL020.md)| More than one interface is matching the name.
| [REFL022](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL022.md)| Use fully qualified name.
| [REFL023](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL023.md)| The type does not implement the interface.
| [REFL024](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL024.md)| Prefer null over empty array.
| [REFL025](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL025.md)| Use correct arguments.
| [REFL026](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL026.md)| No parameterless constructor defined for this object.
| [REFL027](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL027.md)| Prefer Type.EmptyTypes.
| [REFL028](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL028.md)| Cast return value to correct type.
| [REFL029](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL029.md)| Specify types in case an overload is added in the future.
| [REFL030](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL030.md)| Use correct obj parameter.
| [REFL031](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL031.md)| Use generic arguments that satisfies the type parameters.
| [REFL032](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL032.md)| The dependency does not exist.
| [REFL033](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL033.md)| Use the same type as the parameter.
| [REFL034](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL034.md)| Don't call MakeGeneric when not a generic definition.
| [REFL035](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL035.md)| Don't call Invoke on a generic definition.
| [REFL036](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL036.md)| Pass 'throwOnError: true' or check if null.
| [REFL037](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL037.md)| The type does not exist.
| [REFL038](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL038.md)| Prefer RuntimeHelpers.RunClassConstructor.
| [REFL039](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL039.md)| Prefer typeof(...) over instance.GetType when the type is sealed.
| [REFL040](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL040.md)| Prefer type.IsInstanceOfType(...).
| [REFL041](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL041.md)| Delegate type is not matching.
| [REFL042](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL042.md)| First argument must be reference type.
| [REFL043](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL043.md)| First argument must match type.
| [REFL044](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL044.md)| Expected attribute type.
| [REFL045](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL045.md)| These flags are insufficient to match any members.
| [REFL046](https://github.com/DotNetAnalyzers/ReflectionAnalyzers/tree/master/documentation/REFL046.md)| The specified default member does not exist.


## Using ReflectionAnalyzers

The preferable way to use the analyzers is to add the nuget package [ReflectionAnalyzers](https://www.nuget.org/packages/ReflectionAnalyzers)
to the project(s).

The severity of individual rules may be configured using [rule set files](https://msdn.microsoft.com/en-us/library/dd264996.aspx)
in Visual Studio 2015.

## Installation

IDisposableAnalyzers can be installed using:
- [Paket](https://fsprojects.github.io/Paket/) 
- NuGet command line
- NuGet Package Manager in Visual Studio.


**Install using the command line:**
```bash
paket add ReflectionAnalyzers --project <project>
```

or if you prefer NuGet
```bash
Install-Package ReflectionAnalyzers
```
