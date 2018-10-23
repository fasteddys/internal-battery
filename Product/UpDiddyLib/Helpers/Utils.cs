using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        static public string RemoveHTML( string str)
        {
            return Regex.Replace(str, "<.*?>", String.Empty);
        }

        static public string RemoveNewlines(string str)
        {
            return Regex.Replace(str, "\r\n", String.Empty);
        }

        static public string RemoveRedundantSpaces(string str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(str.Trim(), " ");

        }

        static public DateTime UnixMillisecondsToLocalDatetime(long milliseconds)
        { 
            DateTimeOffset DatTimeOff = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToLocalTime();
            return DatTimeOff.DateTime;
        }


    }
}
