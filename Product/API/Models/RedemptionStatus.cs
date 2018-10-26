using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class RedemptionStatus : BaseModel
    {
        public int RedemptionStatusId  { get; set; }
        public Guid RedemptionStatusGuid { get; set; }
        [Required]
        public string Name { get; set; }
    }
}


