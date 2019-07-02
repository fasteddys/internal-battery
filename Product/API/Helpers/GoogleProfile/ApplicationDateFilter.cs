using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ApplicationDateFilter
    {
        public  Date startDate { get; set; }
        public Date endDate { get; set; }
    }
}
