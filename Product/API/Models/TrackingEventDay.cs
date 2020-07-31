using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public class TrackingEventDay: BaseModel
    {
        public int TrackingEventDayId { get; set; }

        [Required]
        public Guid TrackingEventDayGuid { get; set; }

        [Required]
        [Column(TypeName = "Date")]
        public DateTime Day { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        public int TrackingId { get; set; }

        public Tracking Tracking { get; set; }
    }
}
