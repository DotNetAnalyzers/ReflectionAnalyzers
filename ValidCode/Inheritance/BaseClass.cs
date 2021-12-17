namespace ValidCode.Inheritance;

using System;

public class BaseClass
{
    public static int PublicStaticField;

    private static readonly int PrivateStaticField = 1;

    public static event EventHandler? PublicStaticEvent;

    private static event EventHandler? PrivateStaticEvent;

    public static int PublicStaticProperty { get; set; }

    private static int PrivateStaticProperty { get; set; }

    public static int PublicStaticMethod() => 0;

    private static int PrivateStaticMethod() => 0;

    private int M()
    {
        PublicStaticEvent?.Invoke(null, EventArgs.Empty);
        PrivateStaticEvent?.Invoke(null, EventArgs.Empty);
        return PrivateStaticField;
    }
}
