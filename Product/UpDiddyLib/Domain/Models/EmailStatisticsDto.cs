using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyLib.Domain.Models
{
    public class EmailStatisticsDto
    {
        public string MessageId { get; set; }

        public string Subject { get; set; }

        public DateTime? Dropped { get; set; }

        public DateTime? Processed { get; set; }

        [Column("Click")]
        public DateTime? Clicked { get; set; }

        public DateTime? Delivered { get; set; }

        [Column("Bounce")]
        public DateTime? Bounced { get; set; }

        public DateTime? Deferred { get; set; }

        [Column("Open")]
        public DateTime? Opened { get; set; }
    }

    public class EmailStatisticsListDto
    {
        public List<EmailStatisticsDto> EmailStatistics { get; set; } = new List<EmailStatisticsDto>();

        public int TotalRecords => EmailStatistics?.Count ?? 0;
    }

}
