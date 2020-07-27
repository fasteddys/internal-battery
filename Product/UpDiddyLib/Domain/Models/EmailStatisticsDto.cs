using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class EmailStatisticsDto
    {
        public string MessageId { get; set; }

        public string Subject { get; set; }

        public DateTime? Dropped { get; set; }

        public DateTime? Processed { get; set; }

        public DateTime? Clicked { get; set; }

        public DateTime? Delivered { get; set; }

        public DateTime? Bounced { get; set; }

        public DateTime? Deferred { get; set; }

        public DateTime? Opened { get; set; }
    }

    public class EmailStatisticsListDto
    {
        public List<EmailStatisticsDto> EmailStatistics { get; set; } = new List<EmailStatisticsDto>();

        public int TotalRecords => EmailStatistics?.Count ?? 0;
    }

}
