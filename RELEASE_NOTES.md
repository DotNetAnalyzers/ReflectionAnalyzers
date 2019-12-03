#### 0.1.21
* FEATURE Walk a bit more, smarter analyzers in places.
* BUGFIX Handle nameof when open generic type.

#### 0.1.20
* BUGFIX REFL031: conditional expression.
* BUGFIX REFL031: if statement.

#### 0.1.19
* REFL046 check default member.
* Codefix REFL014 don't add flags by default.
* Suggest GetMember().GetAccessor() when not visible.

#### 0.1.18
* REFL025 Nag when passing null.
* REFL025 Missing.Value requires a default value even if the type matches.
* REFL009 don't warn if null checked
* Erroneous REFL037 'The type does not exist' warnings. 

#### 0.1.17
* REFL044 Expected attribute type.
* REFL043 First argument must match type.
* Warn if GetMethod or SetMethod does not exist.

#### 0.1.16
* REFL041 check delegate type when calling Delegate.CreateDelegate.
* REFL040 Prefer type.IsInstanceOfType.
* BUGFIX: NRE in GetTypeAnalyzer when elvis operator.

#### 0.1.15
* REFL014 report when filtered by types.
* REFL039 prefer typeof().
* REFL003 warn on getting named tuple field.
* REFL016 handle ValueTuple.

#### 0.1.14
* REFL038 prefer RuntimeHelpers.RunClassConstructor.
* REFL004 handle static ctor.
* REFL037 handle assembly.GetType

#### 0.1.13
* Fix for REFL004
* BUGFIX REFL004 with indexers.

#### 0.1.12
* BUGFIX: overload resolution.
* REFL016 & REFL017 require nameof when potentially overloading invisible.
* REFL035 Don't call Invoke on a generic definition.
* REFL015 nag when private member is found in base class.

#### 0.1.11
* REFL033 types can be more specific.
* Understand MethodInfo.ReturnType, FieldInfo.FieldType, PropertyInfo.PropertyType
* BUGFIX REFL003 handles overload resolution.
* REFL003 when typeof

#### 0.1.10
* REFL016 should not suggest nameof(Finalize).
* REFL016 should not suggest nameof(op_Equality).
* REFL018 should warn when public not in source.
* Walk some to find out ctor & method info

#### 0.1.9
* BUGFIXES: NRES
* REFL032 check DependencyAttribute
* REFL001 in more places.
* REFL025 in more places.
* REFL031 check MakeGenericX

#### 0.1.8
* Analyzers for Activator.CreateInstance
* BUGFIX REFL018 when not visible member.
* Check arguments when calling MethodInfo.Invoke

#### 0.1.7
* REFL019 Check filter types.
* REFL020 More than one interface is matching the name.
* REFL022 Use fully qualified name.
* REFL023 The type does not implement the interface.
* REFL024 Prefer null over empty array.

#### 0.1.6.1
* REFL016 remove all checks except GetX arguments.
* BUGFIX: REFL016 code action title.
* BUGFIX: use correct member for nameof.
* BUGFIX REFL017 code action message.
* BUGFIX REFL017 and anonymous objects.

#### 0.1.6
* BUGFIX: REFL008 don't warn for types not in sln.
* BUGFIX: Handle anonymous types.
* BUGFIX: nameof when calling GetNestedType.
* BUGFIX nameof when calling inaccessible.
* FEATURE: REFL014 handle events.
* FEATURE: new analyzer don't use nameof.

