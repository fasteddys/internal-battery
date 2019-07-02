using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class TimeFilter
    {
        Timestamp startTime { get; set; }
        Timestamp endTime { get; set; }
        int timeField { get; set; }
    }
}
