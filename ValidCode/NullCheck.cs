// ReSharper disable All
namespace ValidCode;

using System.Reflection;

public class NullCheck
{
    public NullCheck(NullCheck nullCheck)
    {
        var property = nullCheck.GetType().GetProperty("P");
        if (property != null)
        {
        }

        if (nullCheck.GetType().GetProperty("P") is PropertyInfo p)
        {
        }

        _ = nullCheck.GetType().GetMethod("M")?.Invoke(null, null);
    }
}
