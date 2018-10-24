using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        static public string RemoveHTML( string Str)
        {
            return Regex.Replace(Str, "<.*?>", String.Empty);
        }

        static public string RemoveNewlines(string Str)
        {
            return Regex.Replace(Str, "\r\n", String.Empty);
        }

        static public string RemoveRedundantSpaces(string Str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(Str.Trim(), " ");

        }

        static public DateTime UnixMillisecondsToLocalDatetime(long Milliseconds)
        { 
            DateTimeOffset DatTimeOff = DateTimeOffset.FromUnixTimeMilliseconds(Milliseconds).ToLocalTime();
            return DatTimeOff.DateTime;
        }

        static public long CurrentTimeInUnixMilliseconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return rval;
        }

        static public long CurrentTimeInUnixSeconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return rval;
        }


    }
}
