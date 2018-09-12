using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class BaseModel
    {
        public int IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public Guid CreateGuid { get; set; }
        public Guid? ModifyGuid { get; set; }
    }
}
