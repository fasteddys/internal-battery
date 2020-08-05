using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public class Tracking : BaseModel
    {
        public int TrackingId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(2048)")]
        public string Url { get; set; }

        [Column(TypeName = "VARCHAR(150)")]
        public string SourceSlug { get; set; }

        [Required]
        public Guid TrackingGuid { get; set; }

    }
}
