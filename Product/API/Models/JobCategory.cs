using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobCategory : BaseModel
    {

        public int JobCategoryId { get; set; }
        public Guid JobCategoryGuid { get; set; }
        public string Name { get; set; }

    }
}
