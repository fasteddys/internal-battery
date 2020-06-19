using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CommuteDistance: BaseModel
    {
        public int? CommuteDistanceId { get; set; }
        public Guid CommuteDistanceGuid { get; set; }

        [MaxLength(100)]
        public string DistanceRange { get; set; }


    }
}
