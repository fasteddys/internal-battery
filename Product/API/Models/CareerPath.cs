using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CareerPath : BaseModel
    {
        public int CareerPathId { get; set; }
        public Guid CareerPathGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
