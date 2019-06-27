using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class TimeFilter
    {
        Timestamp StartTime { get; set; }
        Timestamp EndTime { get; set; }
        int TimeField { get; set; }
    }
}
