using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class HistogramQueryResult
    {
        public string histogramQuery { get; set; }
        public Dictionary<string,long> histogram { get; set; }
    }
}
