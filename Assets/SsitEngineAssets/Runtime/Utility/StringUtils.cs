using System;
using UnityEngine;

public static class StringUtils
{
    public static DateTime ParseByDefault( this string input, DateTime defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToDateTime(input); }, defaultvalue);
    }

    public static decimal ParseByDefault( this string input, decimal defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToDecimal(input); }, defaultvalue);
    }

    public static double ParseByDefault( this string input, double defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToDouble(input); }, defaultvalue);
    }

    public static int ParseByDefault( this string input, int defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToInt32(input); }, defaultvalue);
    }

    public static long ParseByDefault( this string input, long defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToInt64(input); }, defaultvalue);
    }

    public static float ParseByDefault( this string input, float defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToSingle(input); }, defaultvalue);
    }

    public static short ParseByDefault( this string input, short defaultvalue )
    {
        return input.ParseStringToType(delegate { return Convert.ToInt16(input); }, defaultvalue);
    }

    public static string ParseByDefault( this string input, string defaultvalue )
    {
        if (string.IsNullOrEmpty(input)) return defaultvalue;
        return input;
    }

    public static Vector2 ParseByDefault( this string input, Vector2 defaultvalue )
    {
        return input.ParseStringToType(delegate( string sVector )
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")")) sVector = sVector.Substring(1, sVector.Length - 2);

            // split the items
            var sArray = sVector.Split(',');

            // store as a Vector3
            var result = new Vector2(float.Parse(sArray[0]), float.Parse(sArray[1]));

            return result;
        }, defaultvalue);
    }

    public static Vector3 ParseByDefault( this string input, Vector3 defaultvalue )
    {
        return input.ParseStringToType(delegate( string sVector )
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")")) sVector = sVector.Substring(1, sVector.Length - 2);

            // split the items
            var sArray = sVector.Split(',');

            // store as a Vector3
            var result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            return result;
        }, defaultvalue);
    }

    public static Quaternion ParseByDefault( this string input, Quaternion defaultvalue )
    {
        return input.ParseStringToType(delegate( string sVector )
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")")) sVector = sVector.Substring(1, sVector.Length - 2);

            // split the items
            var s = sVector.Split(',');

            // store as a Vector3

            return new Quaternion(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
        }, defaultvalue);
    }

    public static Color32 ParseByDefault( this string input, Color32 defaultvalue )
    {
        return input.ParseStringToType(delegate( string sVector )
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")")) sVector = sVector.Substring(1, sVector.Length - 2);

            // split the items
            var s = sVector.Split(',');

            // store as a Vector3

            return new Color32(byte.Parse(s[0]), byte.Parse(s[1]), byte.Parse(s[2]), byte.Parse(s[3]));
        }, defaultvalue);
    }

    public static Color ParseByDefault( this string input, Color defaultvalue )
    {
        input = input.Replace("RGBA", "");
        return input.ParseStringToType(delegate( string sVector )
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")")) sVector = sVector.Substring(1, sVector.Length - 2);

            // split the items
            var s = sVector.Split(',');

            // store as a Vector3

            return new Color(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
        }, defaultvalue);
    }

    public static bool ParseByDefault( this string input, bool defaultvalue, string flag = "1" )
    {
        return input.ParseStringToType(delegate( string s )
        {
            // Remove the parentheses
            return s == flag;
        }, defaultvalue);
    }

    public static string DeParseByDefault( this bool input, string defaultvalue = null )
    {
        return input.DeParseTypeToString(delegate( bool s )
        {
            // Remove the parentheses
            return s ? "1" : "0";
        }, defaultvalue);
    }

    private static T ParseStringToType<T>( this string input, Func<string, T> action, T defaultvalue ) where T : struct
    {
        if (string.IsNullOrEmpty(input)) return defaultvalue;
        try
        {
            return action(input);
        }
        catch
        {
            return defaultvalue;
        }
    }

    private static string DeParseTypeToString<T>( this T input, Func<T, string> action, string defaultvalue )
        where T : struct
    {
        try
        {
            return action(input);
        }
        catch
        {
            return defaultvalue;
        }
    }

    public static string JointStringByFormat( params string[] strs )
    {
        if (strs != null && strs.Length > 0) return string.Join("|", strs);

        return null;
    }

    public static string[] SplitStringByFormat( string strs )
    {
        return strs.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
    }
}