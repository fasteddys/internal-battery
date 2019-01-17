using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpDiddyLib.Helpers
{
    public static class ExtensionMethods
    {
        public static TimeSpan DateDiff(this DateTime dt, DateTime compare)
        {
            return compare - dt;
        }
    }
}