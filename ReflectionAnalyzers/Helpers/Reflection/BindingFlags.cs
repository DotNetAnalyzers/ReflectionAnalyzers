namespace ReflectionAnalyzers;

using System;

/// <summary>
/// Mirroring System.Reflection.BindingFlags. See Issue 44.
/// </summary>
[Flags]
internal enum BindingFlags
{
    Default = 0x00,

    /// <summary>
    /// Ignore the case of Names while searching.
    /// </summary>
    IgnoreCase = 0x01,

    /// <summary>
    /// Only look at the members declared on the Type
    /// </summary>
    DeclaredOnly = 0x02,

    /// <summary>
    /// Include Instance members in search
    /// </summary>
    Instance = 0x04,

    /// <summary>
    /// Include Static members in search
    /// </summary>
    Static = 0x08,

    /// <summary>
    /// Include Public members in search
    /// </summary>
    Public = 0x10,

    /// <summary>
    /// Include Non-Public members in search
    /// </summary>
    NonPublic = 0x20,

    /// <summary>
    /// Rollup the statics into the class.
    /// </summary>
    FlattenHierarchy = 0x40,

    // These flags are used by InvokeMember to determine
    // what type of member we are trying to Invoke.
    // BindingAccess = 0xFF00;
    InvokeMethod = 0x0100,
    CreateInstance = 0x0200,
    GetField = 0x0400,
    SetField = 0x0800,
    GetProperty = 0x1000,
    SetProperty = 0x2000,

    /// <summary>
    /// These flags are also used by InvokeMember but they should only be used when calling InvokeMember on a COM object.
    /// </summary>
    PutDispProperty = 0x4000,
    PutRefDispProperty = 0x8000,

    /// <summary>
    /// Bind with Exact Type matching, No Change type
    /// </summary>
    ExactBinding = 0x010000,
    SuppressChangeType = 0x020000,

    /// <summary>
    /// DefaultValueBinding will return the set of methods having ArgCount or more parameters. This is used for default values, etc.
    /// </summary>
    OptionalParamBinding = 0x040000,

    /// <summary>
    /// This is used in COM Interop
    /// </summary>
    IgnoreReturn = 0x01000000,

    DoNotWrapExceptions = 33554432,
}
