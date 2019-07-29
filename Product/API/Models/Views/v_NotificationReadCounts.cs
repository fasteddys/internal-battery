using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_NotificationReadCounts
    {
        public string NotificationTitle { get; set; }
        public DateTime PublishedDate { get; set; }
        public int ReadCount { get; set; }
    }
}
