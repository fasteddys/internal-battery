using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EntityType : BaseModel
    {
        public int EntityTypeId { get; set; }
        public Guid EntityTypeGuid { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
