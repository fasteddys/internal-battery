using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class TrainingType : BaseModel
    {
        public int TrainingTypeId { get; set; }
        public Guid TrainingTypeGuid { get; set; }
        public string Name { get; set; }
        public int? Sequence { get; set; }
    }
}
