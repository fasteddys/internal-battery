using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class RebateType : BaseModel
    {
        public int RebateTypeId { get; set; }
        public Guid? RebateTypeGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
