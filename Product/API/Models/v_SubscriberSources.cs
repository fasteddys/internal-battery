using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    [NotMapped]
    public class v_SubscriberSources
    {
        public string Source { get; set; }
        public string Referrer { get; set; }
        public int Count { get; set; }
    }
}
