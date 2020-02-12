using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SovrenParseStatistic : BaseModel
    {
        public int SovrenParseStatisticId { get; set; }
        public Guid SovrenParseStatisticsGuid { get; set; }

        public long NumTicks { get; set; }

        public int SubscriberId { get; set; }

        public string ResumeText { get; set; }
    }
}
