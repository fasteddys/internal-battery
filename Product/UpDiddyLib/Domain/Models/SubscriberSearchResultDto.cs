﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberSearchResultDto
    {


        public int PageSize { get; set; }
        public int PageNum { get; set; }
        public int SubscriberCount { get; set; }
        public long TotalHits { get; set; }
        public int NumPages { get; set; }

        public double SearchTimeInMilliseconds { get; set; }
        /// <summary>
        /// Time for cloud talent to return search results
        /// </summary>
        public long SearchQueryTimeInTicks { get; set; }
        /// <summary>
        /// Time to map cloud talent search results to cc jobview 
        /// </summary>
        public long SearchMappingTimeInTicks { get; set; }

        public List<SubscriberInfoDto> Subscribers { get; set; } = new List<SubscriberInfoDto>();
    }
}
