using System.Text.RegularExpressions;
using System.Text;

namespace databaseapi.data_access;

public static partial class Ext
{
    public static string GetEncodedName(this string val)
    {
        var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes(val));
        return base64str.RemoveNonAlphaNumeric();
    }

    public static string RemoveNonAlphaNumeric(this string val)
    {
        return NonAlphaNumeric().Replace(val, "");
    }

    [GeneratedRegex("[^a-zA-Z\\d\\s:]")]
    private static partial Regex NonAlphaNumeric();

    public static string ToISO(this DateTime val)
    {
        return $"{val.Year}-{val.Month:00}-{val.Day:00}";
    }
}